using LargeCollections;
using static libkuric.FileFormat.Enums;

namespace libkuric.FileFormat
{
    public class MainHeader
    {
        //Constants
        const uint CMagicWord     = 0x4B494600; //KIF and the last byte 00 for the info byte

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
        public uint            NumMetadateFields { get; set; }
        public uint[]          MetadataFieldLengths { get; set; }
        public LargeList<LargeArray<byte>> MetaData { get; set; }

        public uint GetHeaderLength()
        {
            return 53 + 4 * NumMetadateFields;
        }

        public MemoryStream ToMemoryStream()
        {
            MemoryStream tmpMs = new MemoryStream();
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
            tmpMs.Write(BitConverter.GetBytes(NumMetadateFields));

            for (long i = 0; i < NumMetadateFields; i++)
                tmpMs.Write(BitConverter.GetBytes(MetadataFieldLengths[i]));

            for (long i = 0; i < NumMetadateFields; i++)
                tmpMs.Write(MetaData[i]);

            return tmpMs;
        }

        public void WriteToStream(Stream stream)
        {
            stream.Write(ToMemoryStream().ToArray());
        }

        public void FromStream(Stream stream)
        {
            byte[] readBytes(int length)
            {
                byte[] buff = new byte[length];
                stream.Read(buff, 0, buff.Length);
                return buff;
            }
            
            uint tmp = BitConverter.ToUInt32(readBytes(4));
            if (!tmp.Equals(CMagicWord))
            {
                throw new Exception("Master Header does not start with the magic word!");
            }
            MagicWord = tmp;

            GUID = new Guid(readBytes(16));
            Usage = (Usage)stream.ReadByte();
            Width = BitConverter.ToUInt32(readBytes(4));
            Height = BitConverter.ToUInt32(readBytes(4));
            CompositingColorSpace = (ColorSpace)stream.ReadByte();
            ChDataFormat = (ChDataFormat)stream.ReadByte();
            PPIResH = BitConverter.ToDouble(readBytes(8));
            PPIResV = BitConverter.ToDouble(readBytes(8));
            TileBaseDim = BitConverter.ToUInt16(readBytes(2));
            NumMetadateFields = BitConverter.ToUInt32(readBytes(4));

            MetadataFieldLengths = new uint[NumMetadateFields];
            for (long i = 0; i < NumMetadateFields; i++)
                MetadataFieldLengths[i] = BitConverter.ToUInt32(readBytes(4));
            for (long i = 0; i < NumMetadateFields; i++)
                stream.Read(MetaData[i], 0, MetadataFieldLengths[i]);
        }

        public bool TryReadingFromStream(Stream stream)
        {
            try
            {
                FromStream(stream);
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
