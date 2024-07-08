namespace libvic.FileFormat
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
            JPEG = 0,
            WEBP = 1,
            AVIF = 2,
            JXL  = 3,
            FFV1 = 4,
            HEIC = 5,
            VICLL_None_LZW = 128,
            VICLL_None_ZLIB = 129,
            VICLL_None_BROTLI = 130,
            VICLL_None_XZ = 131,
            VICLL_None_LZMA = 132,
            VICLL_None_AC = 133,
            VICLL_Sub_LZW = 134,
            VICLL_Sub_ZLIB = 135,
            VICLL_Sub_BROTLI = 136,
            VICLL_Sub_XZ = 137,
            VICLL_Sub_LZMA = 138,
            VICLL_Sub_AC = 139,
            VICLL_Up_LZW = 140,
            VICLL_Up_ZLIB = 141,
            VICLL_Up_BORTLI = 142,
            VICLL_Up_XZ = 143,
            VICLL_Up_LZMA = 144,
            VICLL_Up_AC = 145,
            VICLL_Average_LZW = 146,
            VICLL_Average_ZLIB = 147,
            VICLL_Average_BORTLI = 148,
            VICLL_Average_XZ = 149,
            VICLL_Average_LZMA = 150,
            VICLL_Average_AC = 151,
            VICLL_Paeth_LZW = 152,
            VICLL_Paeth_ZLIB = 153,
            VICLL_Paeth_BORTLI = 154,
            VICLL_Paeth_XZ = 155,
            VICLL_Paeth_LZMA = 156,
            VICLL_Paeth_AC = 157,
            VICLL_JXL_LZW = 158,
            VICLL_JXL_ZLIB = 159,
            VICLL_JXL_BORTLI = 160,
            VICLL_JXL_XZ = 161,
            VICLL_JXL_LZMA = 162,
            VICLL_JXL_AC = 163
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
