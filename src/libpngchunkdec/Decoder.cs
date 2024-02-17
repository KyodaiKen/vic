using libpngchunkdec.Headers;
using libpngchunkdec.Filters;
using System.IO.Compression;
using static libpngchunkdec.Enums;

namespace libpngchunkdec
{
    public class Decoder
    {
        //Memory
        Stream          _Stream;
        Image?          _Image;
        Physical?       _Physical;
        byte[]?         _LastIncompleteScanline;
        Chunk?          _CurrentChunk;
        ZLibStream      _ZLibStream;
        bool            _EOF = false;

        //Properties
        public Image?    Image { get { return _Image; } }
        public Physical? Physical { get { return _Physical; } }
        public bool      EOF { get { return _EOF; } }

        public Decoder(Stream stream)
        {
            _Stream = stream;
            _Init();
        }

        public byte[] ReadScanlines(int numScanlines)
        {
            
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
            while (true)
            {
                if (_CurrentChunk?.Type == "IEND" || _Stream.Position >= _Stream.Length - 4)
                    break;
                if (_CurrentChunk?.Type != "IDAT") throw new Exception("???");
                long chunkEnd = _Stream.Position + _CurrentChunk.Length;
                while (_Stream.Position < chunkEnd)
                {
                    byte[] buff = new byte[1];
                    if (_ZLibStream.Read(buff, 0, buff.Length) > 0)
                        scanlinesUnfiltered.Write(buff);
                    else break;
                }
                _Stream.Position += 4;
                _CurrentChunk?.ReadHeader(_Stream);
                if (scanlinesUnfiltered.Length >= expectedLengthWithFilterTypeBytes) break;
            }

            if (_CurrentChunk?.Type == "IEND" || _Stream.Position >= _Stream.Length - 4)
                _EOF = true;

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
            scanlinesUnfiltered.Seek(0, SeekOrigin.Begin);
            byte[] prev = new byte[scanlineLength];
            for (int i = 0; i < expectedLengthWithFilterTypeBytes; i+= scanlineLength + 1)
            {
                Filter flt = (Filter)scanlinesUnfiltered.ReadByte();
                //if ((byte)flt > 4) throw new Exception("Invalit filter designation");
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
                    default:
                        break;
                }
                unfiltered.CopyTo(prev, 0);
                _out.Write(unfiltered);
            }

            scanlinesUnfiltered.Dispose();
            return _out.ToArray();
        }

        void _Init()
        {
            if (_Stream == null) throw new ArgumentNullException(nameof(_Stream));
            if (!_Stream.CanRead) throw new ArgumentException("Stream is not readable", nameof(_Stream));
            if (_Stream.Position + 8 > _Stream.Length) throw new EndOfStreamException();
            //First read the magic word (8 byte);
            if (BitConverter.ToUInt64(_Stream.Read(8)) != Constants.Magic) throw new Exception("This doesn't seem to be a PNG");

            //Read until the first IDAT chunk is encountered
            _CurrentChunk = new();
            _CurrentChunk.ReadHeader(_Stream);
            if (_CurrentChunk.Type != "IHDR") throw new Exception("This is not a valid PNG file");
            _Image = new();
            _Image.FromBytes(_CurrentChunk.ReadData(_Stream));

            do
            {
                _CurrentChunk.ReadHeader(_Stream);
                if (_CurrentChunk.Type == "pHYs")
                {
                    _Physical = new();
                    _Physical.FromBytes(_CurrentChunk.ReadData(_Stream));
                }
                else if(_CurrentChunk.Type != "IDAT")
                {
                    _CurrentChunk.SkipData(_Stream);
                }
            } while (_CurrentChunk.Type != "IDAT");
            _ZLibStream = new(_Stream, CompressionMode.Decompress);
        }
    }
}
