using libpngchunkdec.Headers;
using libpngchunkdec.Filters;
using System.Reflection.Metadata.Ecma335;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.IO.Compression;
using static libpngchunkdec.Enums;

namespace libpngchunkdec
{
    public class Decoder
    {
        //Memory
        Stream          _Stream;
        Image           _Image;
        Physical        _Physical;
        byte[]?         _LastIncompleteScanline;

        //Properties
        public Image    Image { get { return _Image; } }
        public Physical Physical { get { return _Physical; } }

        public Decoder(Stream stream)
        {
            _Stream = stream;
        }

        public byte[] ReadScanlines(int numScanlines)
        {
            Chunk CurrentChunk = new();
            if (_Image == null) CurrentChunk = _Init();
            ArgumentNullException.ThrowIfNull(_Image);
            int bytesPerPixel = _Image.ColorType.NumBytesPerPixel(_Image.BitDepth);
            int scanlineLength = _Image.Width * bytesPerPixel;
            int expectedLength = scanlineLength * numScanlines;
            int expectedLengthWithFilterTypeBytes = (scanlineLength + 1) * numScanlines;
            int read = 0;
            MemoryStream scanlinesUnfiltered = new MemoryStream(expectedLengthWithFilterTypeBytes);

            //Write last incomplete buffer into the stream if there is one.
            if (_LastIncompleteScanline != null) scanlinesUnfiltered.Write(_LastIncompleteScanline);
            _LastIncompleteScanline = null;

            //Walk throgh IDAT chunks until either the file ends, we have enough data or the IEND chunk is reached:
            do
            {
                if (CurrentChunk.Type != "IDAT")
                {
                    CurrentChunk.SkipData(_Stream);
                    continue;
                }
                MemoryStream ChunkData = new(CurrentChunk.ReadData(_Stream));
                ZLibStream compressed = new(ChunkData, CompressionMode.Decompress);
                MemoryStream decompressed = new();
                compressed.CopyTo(decompressed);
                read += (int)decompressed.Length;
                scanlinesUnfiltered.Write(decompressed.ToArray());
                CurrentChunk.ReadHeader(_Stream);
            } while (CurrentChunk.Type != "IEND" && read < expectedLengthWithFilterTypeBytes);

            if (read > expectedLengthWithFilterTypeBytes)
            {
                scanlinesUnfiltered.Seek(expectedLengthWithFilterTypeBytes, SeekOrigin.Begin);
                _LastIncompleteScanline = scanlinesUnfiltered.Read(read - expectedLengthWithFilterTypeBytes);
                scanlinesUnfiltered.SetLength(expectedLengthWithFilterTypeBytes);
            }
            if (read < expectedLengthWithFilterTypeBytes)
                scanlinesUnfiltered.SetLength(expectedLengthWithFilterTypeBytes); //Incomplete image encountered, filling up with zeroes.

            //Unfilter scanlines
            MemoryStream _out = new MemoryStream(expectedLength);
            byte[] prev = new byte[scanlineLength];
            for (int i = 0; i < expectedLengthWithFilterTypeBytes; i+= scanlineLength + 1)
            {
                Filter flt = (Filter)scanlinesUnfiltered.ReadByte();
                byte[] curr = scanlinesUnfiltered.Read(scanlineLength);
                byte[] unfiltered = new byte[scanlineLength];
                switch (flt)
                {
                    case Filter.None:
                        _out.Write(curr);
                        break;
                    case Filter.Sub:
                        for (int col = 0; col < curr.Length; col++)
                            unfiltered[col] = Sub.UnFilter(curr, unfiltered, col, bytesPerPixel);
                        break;
                    case Filter.Up:
                        for (int col = 0; col < curr.Length; col++)
                            unfiltered[col] = Up.UnFilter(curr, prev, col, bytesPerPixel);
                        break;
                    case Filter.Average:
                        for (int col = 0; col < curr.Length; col++)
                            unfiltered[col] = Average.UnFilter(curr, unfiltered, prev, col, bytesPerPixel);
                        break;
                    case Filter.Paeth:
                        for (int col = 0; col < curr.Length; col++)
                            unfiltered[col] = Paeth.UnFilter(curr, unfiltered, prev, col, bytesPerPixel);
                        break;
                    default: break;
                }
                unfiltered.CopyTo(prev, 0);
                _out.Write(unfiltered);
            }

            scanlinesUnfiltered.Dispose();
            return _out.ToArray();
        }

        Chunk _Init()
        {
            if (_Stream == null) throw new ArgumentNullException(nameof(_Stream));
            if (!_Stream.CanRead) throw new ArgumentException("Stream is not readable", nameof(_Stream));
            if (_Stream.Position + 8 > _Stream.Length) throw new EndOfStreamException();
            //First read the magic word (8 byte);
            if (BitConverter.ToUInt64(_Stream.Read(8)) != Constants.Magic) throw new Exception("This doesn't seem to be a PNG");

            //Read until the first IDAT chunk is encountered
            Chunk c = new();
            c.ReadHeader(_Stream);
            if (c.Type != "IHDR") throw new Exception("This is not a valid PNG file");
            _Image = new();
            _Image.FromBytes(c.ReadData(_Stream));

            do
            {
                c.ReadHeader(_Stream);
                if (c.Type == "pHYs")
                {
                    _Physical = new();
                    _Physical.FromBytes(c.ReadData(_Stream));
                }
                else
                {
                    c.SkipData(_Stream);
                }
            } while (c.Type != "IDAT");
            return c;
        }
    }
}
