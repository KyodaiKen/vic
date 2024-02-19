using System.IO.Hashing;

namespace libpngchunkdec
{
    internal class Chunk
    {
        private byte[]      _Data;
        public int Length { get; set; }
        public string Type { get; set; }

        public Chunk(Stream stream)
        {
            Type = "";
            ReadHeader(stream);
        }

        public Chunk() {
            Type = "";
        }

        public void ReadHeader(Stream stream)
        {
            Length = BitConverter.ToInt32(stream.ReadMotorola(4));
            if (stream.Position + 4 <= stream.Length) Type = System.Text.Encoding.ASCII.GetString(stream.Read(4));
        }

        public bool CanReadData(Stream stream)
        {
            return (stream.Position + Length <= stream.Length);
        }

        public byte[] ReadData(Stream stream)
        {
            stream.Position -= 4; //Go back to read the Type again
            _Data = stream.Read(Length + 4);
            var ChunkCRC32 = BitConverter.ToUInt32(stream.ReadMotorola(4));
            var DataCRC32 = Crc32.HashToUInt32(_Data);
            if (DataCRC32 != ChunkCRC32) return [];
            return _Data[4..];
        }

        public void SkipData(Stream stream)
        {
            stream.Position += Length + 4;
        }
    }
}
