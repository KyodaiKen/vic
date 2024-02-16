using static libpngchunkdec.Enums;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace libpngchunkdec.Headers
{
    internal static class Constants
    {
        public const ulong Magic = 0x89504E470D0A1A0A;
    }
    public class Image
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public byte BitDepth { get; set; }
        public ColorType ColorType { get; set; }
        public byte CompressionMethod { get; set; }
        public byte FilterMethod { get; set; }
        public InterlaceMethod InterlaceMethod { get; set; }

        public void FromBytes(byte[] data)
        {
            ArgumentNullException.ThrowIfNull(data);
            if (data.Length != 13) throw new ArgumentException("Data length does not equal 13 bytes", nameof(data));
            Stream m = new MemoryStream(data);
            Width = BitConverter.ToInt32(m.ReadMotorola(4));
            if (Width <= 0) throw new ArgumentException("Invalid width size in header", nameof(data));
            Height = BitConverter.ToInt32(m.ReadMotorola(4));
            if (Height <= 0) throw new ArgumentException("Invalid height size in header", nameof(data));
            BitDepth = (byte)m.ReadByte();
            if ((BitDepth & (BitDepth - 1)) != 0 && BitDepth > 7 && BitDepth < 17) throw new ArgumentException("BitDepth can only be 8 or 16", nameof(data));
            ColorType = (ColorType)m.ReadByte();
            if (ColorType == ColorType.Palette) throw new NotImplementedException("Paletted images are not supported");
            if ((byte)ColorType > 6) throw new ArgumentException("Invalid value in ColorType field", nameof(data));
            CompressionMethod = (byte)m.ReadByte();
            if (CompressionMethod != 0) throw new ArgumentException("Invalid value in CompressionMethod field", nameof(data));
            FilterMethod = (byte)m.ReadByte();
            if (FilterMethod != 0) throw new ArgumentException("Invalid value in FilterMethod field", nameof(data));
            InterlaceMethod = (InterlaceMethod)m.ReadByte();
            if (InterlaceMethod == InterlaceMethod.Adam7) throw new NotImplementedException("Adam7 interlaced images are not supported");
        }
    }

    public class Physical
    {
        public uint PPUX { get; set; }
        public uint PPUY { get; set; }
        public byte Unit { get; set; }

        public void FromBytes(byte[] data)
        {
            ArgumentNullException.ThrowIfNull(data);
            if (data.Length != 9) throw new ArgumentException("Data length does not equal 9 bytes", nameof(data));
            Stream m = new MemoryStream(data);
            PPUX = BitConverter.ToUInt32(m.ReadMotorola(4));
            PPUY = BitConverter.ToUInt32(m.ReadMotorola(4));
            Unit = (byte)m.ReadByte();
            if (Unit != 1) throw new ArgumentException("Field Unit contains an invalid value.", nameof(data));
        }
    }
}
