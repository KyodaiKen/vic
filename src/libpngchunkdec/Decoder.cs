using libpngchunkdec.Headers;
using libpngchunkdec.Filters;
using System.Reflection.Metadata.Ecma335;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace libpngchunkdec
{
    public class Decoder
    {
        //Memory
        Image           _Image;
        Physical        _Physical;
        byte[]          CurrentChunk;
        int             CurrentScanline;

        //Properties
        public Image    Image { get { return _Image; } }
        public Physical Physical { get { return _Physical; } }

        public byte[] ReadScanlines(Stream stream, int numScanlines)
        {
            if (_Image == null) _Init(stream);
            ArgumentNullException.ThrowIfNull(_Image);
            MemoryStream _scanlines = new MemoryStream(numScanlines * _Image.Width * _Image.ColorType.NumBytesPerPixel(_Image.BitDepth));
            

            return _scanlines.ToArray();
        }

        void _Init(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (!stream.CanRead) throw new ArgumentException("Stream is not readable", nameof(stream));
            if (stream.Position + 8 > stream.Length) throw new EndOfStreamException();
            //First read the magic word (8 byte);
            if (BitConverter.ToUInt64(stream.Read(8)) != Constants.Magic) throw new Exception("This doesn't seem to be a PNG");

            Chunk c = new();
            c.ReadHeader(stream);
            if (c.Type != "IHDR") throw new Exception("This is not a valid PNG file");
            _Image = new();
            _Image.FromBytes(c.ReadData(stream));

            do
            {
                c.ReadHeader(stream);
                if (c.Type == "pHYs")
                {
                    _Physical = new();
                    _Physical.FromBytes(c.ReadData(stream));
                }
                else
                {
                    c.SkipData(stream);
                }
            } while (c.Type != "IDAT");
        }
    }
}
