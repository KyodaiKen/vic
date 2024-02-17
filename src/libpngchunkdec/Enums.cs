namespace libpngchunkdec
{
    public static class Enums
    {
        public enum ColorType : byte
        {
            Gray = 0,
            RGB = 2,
            Palette = 3,
            GrayAlpha = 4,
            RGBA = 6
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
