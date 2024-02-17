using libpngchunkdec.Headers;
using libpngchunkdec.Filters;
using System.IO.Compression;
using static libpngchunkdec.Enums;

namespace libpngchunkdec
{
    public class Decoder : IDisposable
    {
        //Memory
        Stream          _Stream;
        Image?          _Image;
        Physical?       _Physical;
        byte[]?         _LastOverlap;
        byte[]?         _LastDecodedScanline;
        Chunk?          _CurrentChunk;
        MemoryStream    _ZLibInput;
        ZLibStream      _ZLibStream;
        bool            _EOF = false;
        private bool    disposedValue;

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
            MemoryStream scanlinesUnfiltered = new MemoryStream();

            //Write last incomplete buffer into the stream if there is one.
            if (_LastOverlap != null)
            {
                scanlinesUnfiltered.Write(_LastOverlap);
                _LastOverlap = null;
            }
                        
            //Walk throgh IDAT chunks until either the file ends, we have enough data or the IEND chunk is reached:
            while (true)
            {
                if (_CurrentChunk?.Type == "IEND" || _Stream.Position >= _Stream.Length - 4)
                    break;
                if (_CurrentChunk?.Type != "IDAT") throw new Exception("???");
                _ZLibInput.Write(_Stream.Read(_CurrentChunk.Length));
                _ZLibInput.Position = 0;
                MemoryStream tmp = new MemoryStream();
                _ZLibStream.CopyTo(tmp);
                _ZLibInput.SetLength(0);
                scanlinesUnfiltered.Write(tmp.ToArray());
                tmp.Dispose();
                _Stream.Position += 4;
                _CurrentChunk?.ReadHeader(_Stream);
                if (scanlinesUnfiltered.Length >= expectedLengthWithFilterTypeBytes) break;
            }

            if (_CurrentChunk?.Type == "IEND" || _Stream.Position >= _Stream.Length - 4)
                _EOF = true;

            if (scanlinesUnfiltered.Length > expectedLengthWithFilterTypeBytes)
            {
                scanlinesUnfiltered.Seek(expectedLengthWithFilterTypeBytes, SeekOrigin.Begin);
                _LastOverlap = scanlinesUnfiltered.Read(scanlinesUnfiltered.Length - expectedLengthWithFilterTypeBytes);
            } else if (scanlinesUnfiltered.Length < expectedLengthWithFilterTypeBytes)
                scanlinesUnfiltered.SetLength(expectedLengthWithFilterTypeBytes); //Incomplete image encountered, filling up with zeroes.

            //Unfilter scanlines
            MemoryStream _out = new MemoryStream(expectedLength);
            scanlinesUnfiltered.Seek(0, SeekOrigin.Begin);
            if(_LastDecodedScanline == null) _LastDecodedScanline = new byte[scanlineLength];
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
                            unfiltered[col] = Up.UnFilter(curr, _LastDecodedScanline, col, bytesPerPixel);
                        break;
                    case Filter.Average:
                        for (int col = 0; col < curr.Length; col++)
                            unfiltered[col] = Average.UnFilter(curr, unfiltered, _LastDecodedScanline, col, bytesPerPixel);
                        break;
                    case Filter.Paeth:
                        for (int col = 0; col < curr.Length; col++)
                            unfiltered[col] = Paeth.UnFilter(curr, unfiltered, _LastDecodedScanline, col, bytesPerPixel);
                        break;
                    default:
                        break;
                }
                unfiltered.CopyTo(_LastDecodedScanline, 0);
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
            _ZLibInput = new();
            _ZLibStream = new(_ZLibInput, CompressionMode.Decompress);
        }

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _ZLibStream.Dispose();
                    _ZLibInput.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                _LastOverlap = null;
                _LastDecodedScanline = null;
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~Decoder()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
