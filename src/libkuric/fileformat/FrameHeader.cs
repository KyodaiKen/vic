using LargeCollections;
using libkuric;
using System.Numerics;
using System.Text;

namespace libkuric.FileFormat
{
    public class FrameHeader
    {
        //Constants
        const uint CMagicWord = 0x4B4652FF; //KFR + FF

        //Fields
        public uint         MagicWord                   { get; set; } = CMagicWord;
        public ulong        FrameSeqNbr                 { get; set; }
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
