using static libpngchunkdec.Enums;

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

        public static int NumBytesPerPixel(this ColorType ct, byte bpc = 8)
        {
            switch (ct)
            {
                case ColorType.Gray:        return 1 * bpc / 8;
                case ColorType.RGB:         return 3 * bpc / 8;
                case ColorType.GrayAlpha:   return 2 * bpc / 8;
                case ColorType.RGBA:        return 4 * bpc / 8;
                default: return 0;
            }
        }
    }
}
