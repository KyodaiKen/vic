namespace libkuric.FileFormat
{
    public static class Enums
    {
        public enum ColorSpace : byte
        {
            GRAY,
            GRAYA_Straight,
            GRAYA_PreMult,
            RGB,
            RGBA_Straight,
            RGBA_PreMult,
            CMYK,
            CMYKA_Straight,
            CMYKA_PreMult,
            YCrCb,
            YCrCbA_Straight,
            YCrCbA_PreMult
        }

        public enum ChDataFormat : byte
        {
            UINT8,
            UINT16,
            UINT32,
            UINT64,
            UINT128,
            FLOAT16,
            FLOAT32,
            FLOAT64,
            FLOAT128
        }

        public enum Usage : byte
        {
            Gallery,
            Animation
        }

        public enum LayerBlendMode : byte
        {
            Normal,
            Multiply,
            Divide,
            Add,
            Subtract
        }

        public enum TileCompression : byte
        {
            None = 0,
            JPEG = 1,
            WEBP = 2,
            AVIF = 3,
            JXL  = 4,
            KURIF_FAST = 253,
            KURIF_MEDIUM = 254,
            KURIF_SLOW = 255
        }

        #region Determine_Entity_Counts
        static int NumEntities(ChDataFormat df)
        {
            switch (df)
            {
                case ChDataFormat.UINT8:
                    return 1;
                case ChDataFormat.UINT16:
                case ChDataFormat.FLOAT16:
                    return 2;
                case ChDataFormat.UINT32:
                case ChDataFormat.FLOAT32:
                    return 4;
                case ChDataFormat.UINT64:
                case ChDataFormat.FLOAT64:
                    return 8;
                case ChDataFormat.FLOAT128:
                case ChDataFormat.UINT128:
                    return 16;
                default:
                    return -1;
            }
        }
        static int NumEntities(ColorSpace cs)
        {
            switch (cs)
            {
                case ColorSpace.GRAY:
                    return 1;
                case ColorSpace.GRAYA_PreMult:
                case ColorSpace.GRAYA_Straight:
                    return 2;
                case ColorSpace.RGB:
                case ColorSpace.YCrCb:
                    return 3;
                case ColorSpace.RGBA_PreMult:
                case ColorSpace.RGBA_Straight:
                case ColorSpace.YCrCbA_Straight:
                case ColorSpace.YCrCbA_PreMult:
                case ColorSpace.CMYK:
                    return 4;
                case ColorSpace.CMYKA_Straight:
                case ColorSpace.CMYKA_PreMult:
                    return 5;
                default:
                    return -1;
            }
        }
        #endregion
    }
}
