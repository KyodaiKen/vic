using libpngchunkdec.Headers;
using libpngchunkdec.Filters;
using System.IO.Compression;
using static libpngchunkdec.Enums;
using System.Runtime.Intrinsics.Arm;

namespace libpngchunkdec
{
    public class Decoder : IDisposable
    {
        //Memory
        private Stream          _Stream;
        private MemoryStream    _ZLibInput;
        private MemoryStream    _Temp;
        private ZLibStream      _ZLibStream;
        private MemoryStream    _ScanlinesUnfiltered;

        private Image?          _Image;
        private Physical?       _Physical;
        private Chunk?          _CurrentChunk;

        private byte[]          _LastOverlap;
        private byte[]          _LastDecodedScanline;
        private byte[]          _Unfiltered;
        private byte[]          _CurrentScanline;

        private bool            _EOF = false;
        private bool            _disposedValue;

        //Properties
        public Image?    Image { get { return _Image; } }
        public Physical? Physical { get { return _Physical; } }
        public bool      EOF { get { return _EOF; } }

        public Decoder(Stream stream)
        {
            _Stream = stream;
            _Init();
        }

        public void ReadScanlines(Stream OutputStream, int numScanlines)
        {
            ArgumentNullException.ThrowIfNull(_Image);
            int bytesPerPixel = _Image.ColorType.NumBytesPerPixel(_Image.BitDepth);
            int scanlineLength = _Image.Width * bytesPerPixel;
            int expectedLength = scanlineLength * numScanlines;
            int expectedLengthWithFilterTypeBytes = (scanlineLength + 1) * numScanlines;
            if (_Unfiltered == null || _Unfiltered.Length == 0) _Unfiltered = new byte[scanlineLength];
            if (_CurrentScanline == null || _CurrentScanline.Length == 0) _CurrentScanline = new byte[scanlineLength];

            if (_ScanlinesUnfiltered.Length > 0) _ScanlinesUnfiltered.SetLength(0);
            //Write last incomplete buffer into the stream if there is one.
            if (_LastOverlap != null)
            {
                _ScanlinesUnfiltered.Write(_LastOverlap);
                _LastOverlap = new byte[0];
            }
            
            //Walk throgh IDAT chunks until either the file ends, we have enough data or the IEND chunk is reached:
            while (true)
            {
                if (_CurrentChunk?.Type == "IEND" || _Stream.Position >= _Stream.Length - 4)
                    break;
                if (_CurrentChunk?.Type != "IDAT") throw new Exception("???");
                _ZLibInput.Write(_Stream.Read(_CurrentChunk.Length));
                _ZLibInput.Position = 0;
                if (_Temp == null || _Temp.Length != _CurrentChunk.Length)
                    _Temp = new(_CurrentChunk.Length);
                else
                    _Temp.Position = 0;
                _ZLibStream.CopyTo(_Temp);
                _ZLibInput.SetLength(0);
                _ScanlinesUnfiltered.Write(_Temp.ToArray());
                _Stream.Position += 4;
                _CurrentChunk?.ReadHeader(_Stream);
                if (_ScanlinesUnfiltered.Length >= expectedLengthWithFilterTypeBytes) break;
            }

            if (_CurrentChunk?.Type == "IEND" || _Stream.Position >= _Stream.Length - 4)
                _EOF = true;

            if (_ScanlinesUnfiltered.Length > expectedLengthWithFilterTypeBytes)
            {
                _ScanlinesUnfiltered.Seek(expectedLengthWithFilterTypeBytes, SeekOrigin.Begin);
                _LastOverlap = _ScanlinesUnfiltered.Read(_ScanlinesUnfiltered.Length - expectedLengthWithFilterTypeBytes);
            } else if (_ScanlinesUnfiltered.Length < expectedLengthWithFilterTypeBytes)
                _ScanlinesUnfiltered.SetLength(expectedLengthWithFilterTypeBytes); //Incomplete image encountered, filling up with zeroes.

            //Unfilter scanlines
            Filter flt;
            _ScanlinesUnfiltered.Seek(0, SeekOrigin.Begin);
            if(_LastDecodedScanline == null) _LastDecodedScanline = new byte[scanlineLength];
            for (int i = 0; i < expectedLengthWithFilterTypeBytes; i+= scanlineLength + 1)
            {
                flt = (Filter)_ScanlinesUnfiltered.ReadByte();
                //if ((byte)flt > 4) throw new Exception("Invalit filter designation");
                _ScanlinesUnfiltered.Read(_CurrentScanline, 0, scanlineLength);
                switch (flt)
                {
                    case Filter.None:
                        OutputStream.Write(_CurrentScanline);
                        _CurrentScanline.CopyTo(_LastDecodedScanline, 0);
                        break;
                    case Filter.Sub:
                        for (int col = 0; col < _CurrentScanline.Length; col++)
                            _Unfiltered[col] = Sub.UnFilter(_CurrentScanline, _Unfiltered, col, bytesPerPixel);
                        break;
                    case Filter.Up:
                        for (int col = 0; col < _CurrentScanline.Length; col++)
                            _Unfiltered[col] = Up.UnFilter(_CurrentScanline, _LastDecodedScanline, col, bytesPerPixel);
                        break;
                    case Filter.Average:
                        for (int col = 0; col < _CurrentScanline.Length; col++)
                            _Unfiltered[col] = Average.UnFilter(_CurrentScanline, _Unfiltered, _LastDecodedScanline, col, bytesPerPixel);
                        break;
                    case Filter.Paeth:
                        for (int col = 0; col < _CurrentScanline.Length; col++)
                            _Unfiltered[col] = Paeth.UnFilter(_CurrentScanline, _Unfiltered, _LastDecodedScanline, col, bytesPerPixel);
                        break;
                    default:
                        break;
                }
                _Unfiltered.CopyTo(_LastDecodedScanline, 0);
                OutputStream.Write(_Unfiltered);
            }
        }

        private void _Init()
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
            _ScanlinesUnfiltered = new();
            _ZLibStream = new(_ZLibInput, CompressionMode.Decompress);
        }

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _ZLibStream.Dispose();
                    _ZLibInput.Dispose();
                    _Temp.Dispose();
                    _ScanlinesUnfiltered.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                _LastOverlap = null;
                _LastDecodedScanline = null;
                _disposedValue = true;
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
