using static libkuric.FileFormat.Enums;

namespace libkuric.FileFormat
{
    internal class TileHeader
    {
        //Constants
        const uint CMagicWord = 0x4B4C52FF; //KLR + 0xFF

        //Fields
        uint MagicWord = CMagicWord;
        uint TileDataLength { get; set; }
        TileCompression TileDataCompression { get; set; }

        public MemoryStream ToMemoryStream()
        {
            MemoryStream tmpMs = new MemoryStream();
            tmpMs.Write(BitConverter.GetBytes(MagicWord));
            tmpMs.Write(BitConverter.GetBytes(TileDataLength));
            tmpMs.WriteByte((byte)TileDataCompression);
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
            TileDataLength = BitConverter.ToUInt16(stream.Read(4));
            TileDataCompression = (TileCompression)stream.ReadByte();
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
