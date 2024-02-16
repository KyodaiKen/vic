namespace libpngchunkdec
{
    public static class Enums
    {
        public enum ColorType : byte
        {
            Gray,
            RGB,
            Palette,
            GrayAlpha,
            RGBA
        }

        public enum InterlaceMethod : byte
        {
            Progressive,
            Adam7
        }
    }
}
