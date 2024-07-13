using static libvic.FileFormat.Enums;

namespace libvic.FileFormat.Headers
{
    public class TileHeader
    {
        //Fields
        TileAlgorithm TileComprAlgorithm { get; set; }
        TileFlags TileFlags { get; set; }
        uint TileDataLength { get; set; }

        public MemoryStream ToMemoryStream()
        {
            MemoryStream tmpMs = new MemoryStream();
            tmpMs.WriteByte((byte)TileComprAlgorithm);
            tmpMs.WriteByte((byte)TileFlags);
            tmpMs.Write(BitConverter.GetBytes(TileDataLength));
            return tmpMs;
        }

        public void WriteToStream(Stream stream)
        {
            stream.Write(ToMemoryStream().ToArray());
        }

        public void ReadFromStream(Stream stream)
        {
            TileComprAlgorithm = (TileAlgorithm)stream.ReadByte();
            TileFlags = (TileFlags)stream.ReadByte();
            TileDataLength = BitConverter.ToUInt16(stream.Read(4));
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
