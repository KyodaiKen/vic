using LargeCollections;
using libvic;
using static libvic.FileFormat.Enums;

namespace libvic.FileFormat.Headers
{
    public class ImgObjHeader
    {
        //Constants
        const uint CMagicWord = 0x494F424A; //IOBJ

        //Fields
        public uint MagicWord { get; set; } = CMagicWord;
        public ulong FrameSeqNbr { get; set; }
        public ImgObjType Type { get; set; }
        public uint Width { get; set; }
        public uint Height { get; set; }
        public ColorSpace CompositingColorSpace { get; set; }
        public ChDataFormat ChDataFormat { get; set; }
        public double PPIResH { get; set; }
        public double PPIResV { get; set; }
        public ushort TileBaseDim { get; set; }
        public uint DisplayDuration { get; set; }
        public uint NumMetadataFields { get; set; }
        public ushort[] MetadataFieldLengths { get; set; }
        public LargeList<LargeArray<byte>> MetaData { get; set; }

        ImgObjHeader()
        {
            MetadataFieldLengths = [];
            MetaData = [];
        }

        public MemoryStream ToMemoryStream()
        {
            MemoryStream tmpMs = new();
            tmpMs.Write(BitConverter.GetBytes(MagicWord));
            tmpMs.Write(BitConverter.GetBytes(FrameSeqNbr));
            tmpMs.WriteByte((byte)Type);
            tmpMs.Write(BitConverter.GetBytes(Width));
            tmpMs.Write(BitConverter.GetBytes(Height));
            tmpMs.WriteByte((byte)CompositingColorSpace);
            tmpMs.WriteByte((byte)ChDataFormat);
            tmpMs.Write(BitConverter.GetBytes(PPIResH));
            tmpMs.Write(BitConverter.GetBytes(PPIResV));
            tmpMs.Write(BitConverter.GetBytes(TileBaseDim));
            tmpMs.Write(BitConverter.GetBytes(DisplayDuration));
            tmpMs.Write(BitConverter.GetBytes(NumMetadataFields));
            for (uint i = 0; i < NumMetadataFields; i++)
                tmpMs.Write(BitConverter.GetBytes(MetadataFieldLengths[i]));
            for (int i = 0; i < NumMetadataFields; i++) tmpMs.Write(MetaData[i]);
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
                throw new Exception("ImageObject Header does not start with the magic word!");
            }
            FrameSeqNbr = BitConverter.ToUInt64(stream.Read(8));
            Type = (ImgObjType)stream.ReadByte();
            Width = BitConverter.ToUInt32(stream.Read(4));
            Height = BitConverter.ToUInt32(stream.Read(4));
            CompositingColorSpace = (ColorSpace)stream.ReadByte();
            ChDataFormat = (ChDataFormat)stream.ReadByte();
            PPIResH = BitConverter.ToDouble(stream.Read(8));
            PPIResV = BitConverter.ToDouble(stream.Read(8));
            TileBaseDim = BitConverter.ToUInt16(stream.Read(2));
            DisplayDuration = BitConverter.ToUInt32(stream.Read(4));
            NumMetadataFields = BitConverter.ToUInt32(stream.Read(4));
            for (uint i = 0; i < NumMetadataFields; i++)
                MetadataFieldLengths[i] = BitConverter.ToUInt16(stream.Read(4));
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
