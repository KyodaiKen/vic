namespace libvic.FileFormat
{
    public static class Enums
    {
        public enum Usage : byte
        {
            ImageCollection,
            Animation
        }
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
            YCrCbA_PreMult,
            Custom //Metadata must contain field "num_channels" > 0 <= 255
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

        public enum ImgObjType : byte
        {
            KeyFrame,
            Predicted,
            BiDirectionalPred
        }

        public enum LayerBlendMode : byte
        {
            Normal,
            Multiply,
            Divide,
            Add,
            Subtract
        }

        public enum TileAlgorithm : byte
        {
            JPEG = 0,
            WEBP = 1,
            AVIF = 2,
            JXL  = 3,
            FFV1 = 4,
            HEIC = 5,
            VICLL_LZW = 128,
            VICLL_ZLIB = 129,
            VICLL_BROTLI = 130,
            VICLL_XZ = 131,
            VICLL_LZMA = 132,
            VICLL_AC = 133
        }

        public enum TileFlags : byte
        {
            VICLL_FLT_None = 0x80,
            VICLL_FLT_Sub = 0x81,
            VICLL_FLT_Up = 0x82,
            VICLL_FLT_Average = 0x83,
            VICLL_FLT_Median = 0x84,
            VICLL_FLT_Median2 = 0x85,
            VICLL_FLT_Paeth = 0x86,
            VICLL_FLT_JXLPred = 0x87
        }

        #region Determine_Entity_Counts
        static int NumEntities(this ChDataFormat df)
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
        static int NumEntities(this ColorSpace cs)
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
