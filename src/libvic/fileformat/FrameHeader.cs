using LargeCollections;
using System.Text;
using static libvic.FileFormat.Enums;

namespace libvic.FileFormat
{
    public class FrameHeader
    {
        //Constants
        const uint CMagicWord = 0x564652FB; //VFR + FB

        //Fields
        public uint         MagicWord                   { get; set; } = CMagicWord;
        public ulong        FrameSeqNbr                 { get; set; }
        public uint         Width                       { get; set; }
        public uint         Height                      { get; set; }
        public ColorSpace   CompositingColorSpace       { get; set; }
        public ChDataFormat ChDataFormat                { get; set; }
        public double       PPIResH                     { get; set; }
        public double       PPIResV                     { get; set; }
        public ushort       TileBaseDim                 { get; set; }
        public byte         FrameNameLen                { get; set; }
        public uint         FrameDescriptionLen         { get; set; }
        public uint         DisplayDuration             { get; set; }
        public uint         NumMetadataFields           { get; set; }
        public ushort[]     MetadataFieldLengths        { get; set; }
        public string       FrameName                   { get; set; }
        public string       FrameDescription            { get; set; }
        public LargeList<LargeArray<byte>> MetaData     { get; set; }

        FrameHeader()
        {
            MetadataFieldLengths = [];
            FrameName = string.Empty;
            FrameDescription = string.Empty;
            MetaData = [];
        }

        public MemoryStream ToMemoryStream()
        {
            MemoryStream tmpMs = new();
            tmpMs.Write(BitConverter.GetBytes(MagicWord));
            tmpMs.Write(BitConverter.GetBytes(FrameSeqNbr));
            tmpMs.Write(BitConverter.GetBytes(Width));
            tmpMs.Write(BitConverter.GetBytes(Height));
            tmpMs.WriteByte((byte)CompositingColorSpace);
            tmpMs.WriteByte((byte)ChDataFormat);
            tmpMs.Write(BitConverter.GetBytes(PPIResH));
            tmpMs.Write(BitConverter.GetBytes(PPIResV));
            tmpMs.Write(BitConverter.GetBytes(TileBaseDim));
            tmpMs.WriteByte(FrameNameLen);
            tmpMs.Write(BitConverter.GetBytes(FrameDescriptionLen));
            tmpMs.Write(BitConverter.GetBytes(NumMetadataFields));
            tmpMs.Write(BitConverter.GetBytes(DisplayDuration));
            for (uint i = 0; i < NumMetadataFields; i++)
                tmpMs.Write(BitConverter.GetBytes(MetadataFieldLengths[i]));
            if (FrameName.Length > byte.MaxValue) FrameName = FrameName[..byte.MaxValue];
            if (FrameDescription.Length > ushort.MaxValue) FrameDescription = FrameDescription[..ushort.MaxValue];
            tmpMs.Write(Encoding.UTF8.GetBytes(FrameName));
            tmpMs.Write(Encoding.UTF8.GetBytes(FrameDescription));
            for (int i = 0; (i < NumMetadataFields); i++) tmpMs.Write(MetaData[i]);
            return tmpMs;
        }
        public void WriteToStream(Stream stream)
        {
            stream.Write(ToMemoryStream().ToArray());
        }

        public void ReadFromStream(Stream stream)
        {
            uint tmp = BitConverter.ToUInt32(stream.Read(4));
            if (!tmp.Equals(CMagicWord))
            {
                throw new Exception("Frame Header does not start with the magic word!");
            }
            FrameSeqNbr = BitConverter.ToUInt64(stream.Read(8));
            Width = BitConverter.ToUInt32(stream.Read(4));
            Height = BitConverter.ToUInt32(stream.Read(4));
            CompositingColorSpace = (ColorSpace)stream.ReadByte();
            ChDataFormat = (ChDataFormat)stream.ReadByte();
            PPIResH = BitConverter.ToDouble(stream.Read(8));
            PPIResV = BitConverter.ToDouble(stream.Read(8));
            TileBaseDim = BitConverter.ToUInt16(stream.Read(2));
            FrameNameLen = (byte)stream.ReadByte();
            FrameDescriptionLen = BitConverter.ToUInt32(stream.Read(2));
            DisplayDuration = BitConverter.ToUInt32(stream.Read(4));
            NumMetadataFields = BitConverter.ToUInt32(stream.Read(4));
            for (uint i= 0; i < NumMetadataFields; i++)
                MetadataFieldLengths[i] = BitConverter.ToUInt16(stream.Read(4));
            FrameName = Encoding.UTF8.GetString(stream.Read(FrameNameLen));
            FrameDescription = Encoding.UTF8.GetString(stream.Read((int)FrameDescriptionLen));
            MetaData = [];
            for (long i = 0; i < NumMetadataFields; i++)
                stream.Read(MetaData[i], 0, MetadataFieldLengths[i]);
        }

        public bool TryReadingFromStream(Stream stream)
        {
            try
            {
                ReadFromStream(stream);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
