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

        public enum Filter : byte
        {
            None,
            Sub,
            Up,
            Average,
            Paeth
        }
    }
}
