namespace libpngchunkdec
{
    public static class Helpers
    {
        public static byte[] Read(this Stream stream, int length)
        {
            byte[] buff = new byte[length];
            stream.Read(buff, 0, buff.Length);
            return buff;
        }

        public static byte[] ReadMotorola(this Stream stream, int length)
        {
            byte[] buff = new byte[length];
            stream.Read(buff, 0, buff.Length);
            Array.Reverse(buff);
            return buff;
        }
    }
}
