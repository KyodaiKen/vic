using static libpngchunkdec.Enums;

namespace libpngchunkdec
{
    public static class Helpers
    {
        public static byte[] Read(this Stream stream, long length)
        {
            byte[] buff = new byte[length];
            stream.Read(buff, 0, buff.Length);
            return buff;
        }

        public static byte[] ReadMotorola(this Stream stream, long length)
        {
            byte[] buff = new byte[length];
            stream.Read(buff, 0, buff.Length);
            Array.Reverse(buff);
            return buff;
        }

        public static int NumBytesPerPixel(this ColorType ct, byte bpc = 8)
        {
            return ct switch
            {
                ColorType.Gray => 1 * bpc / 8,
                ColorType.RGB => 3 * bpc / 8,
                ColorType.GrayAlpha => 2 * bpc / 8,
                ColorType.RGBA => 4 * bpc / 8,
                ColorType.Palette => 0,
                _ => 0,
            };
        }
    }
}
