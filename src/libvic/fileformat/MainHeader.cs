using LargeCollections;
using static libvic.FileFormat.Enums;

namespace libvic.FileFormat
{
    public class MainHeader
    {
        //Constants
        const uint CMagicWord = 0x56494300; //VIC and the last byte 00 for the info byte

        //Fields
        public uint            MagicWord { get; set; } = CMagicWord;
        public Guid            GUID { get; set; }
        public Usage           Usage { get; set; }
        public uint            Width { get; set; }
        public uint            Height { get; set; }
        public ColorSpace      CompositingColorSpace { get; set; }
        public ChDataFormat    ChDataFormat { get; set; }
        public double          PPIResH { get; set; }
        public double          PPIResV { get; set; }
        public ushort          TileBaseDim { get; set; }
        public uint            NumMetadataFields { get; set; }
        public uint[]          MetadataFieldLengths { get; set; }
        public LargeList<LargeArray<byte>> MetaData { get; set; }

        MainHeader()
        {
            MetadataFieldLengths = [];
            MetaData = [];
        }

        public MemoryStream ToMemoryStream()
        {
            MemoryStream tmpMs = new();
            tmpMs.Write(BitConverter.GetBytes(MagicWord));
            tmpMs.Write(GUID.ToByteArray());
            tmpMs.WriteByte((byte)Usage);
            tmpMs.Write(BitConverter.GetBytes(Width));
            tmpMs.Write(BitConverter.GetBytes(Height));
            tmpMs.WriteByte((byte)CompositingColorSpace);
            tmpMs.WriteByte((byte)ChDataFormat);
            tmpMs.Write(BitConverter.GetBytes(PPIResH));
            tmpMs.Write(BitConverter.GetBytes(PPIResV));
            tmpMs.Write(BitConverter.GetBytes(TileBaseDim));
            tmpMs.Write(BitConverter.GetBytes(NumMetadataFields));
            for (long i = 0; i < NumMetadataFields; i++)
                tmpMs.Write(BitConverter.GetBytes(MetadataFieldLengths[i]));
            for (long i = 0; i < NumMetadataFields; i++)
                tmpMs.Write(MetaData[i]);
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
                throw new Exception("Master Header does not start with the magic word!");
            }
            GUID = new Guid(stream.Read(16));
            Usage = (Usage)stream.ReadByte();
            Width = BitConverter.ToUInt32(stream.Read(4));
            Height = BitConverter.ToUInt32(stream.Read(4));
            CompositingColorSpace = (ColorSpace)stream.ReadByte();
            ChDataFormat = (ChDataFormat)stream.ReadByte();
            PPIResH = BitConverter.ToDouble(stream.Read(8));
            PPIResV = BitConverter.ToDouble(stream.Read(8));
            TileBaseDim = BitConverter.ToUInt16(stream.Read(2));
            NumMetadataFields = BitConverter.ToUInt32(stream.Read(4));
            MetadataFieldLengths = new uint[NumMetadataFields];
            for (long i = 0; i < NumMetadataFields; i++)
                MetadataFieldLengths[i] = BitConverter.ToUInt32(stream.Read(4));
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

        public bool GenUUID()
        {
            try
            {
                GUID = Guid.NewGuid();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
