namespace libpngchunkdec
{
    internal class Chunk
    {
        public int Length { get; set; }
        public string Type { get; set; }

        public Chunk(Stream stream)
        {
            Type = "";
            ReadHeader(stream);
        }

        public Chunk() {
            Length = 0;
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
            byte[] data = stream.Read(Length);
            stream.Position += 4; //Ignore CRC
            return data;
        }

        public void SkipData(Stream stream)
        {
            stream.Position += Length + 4;
        }
    }
}
