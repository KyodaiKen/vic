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
#pragma warning disable IDE0044
        private Stream          _InputStream;
        private Stream          _OutputStream;
        private MemoryStream    _ZLibInput;
        private MemoryStream    _Temp;
        private ZLibStream      _ZLibStream;
        private MemoryStream    _ScanlinesUnfiltered;

        private Image           _Image;
        private Physical?       _Physical;
        private Chunk           _CurrentChunk;

        private byte[]          _LastOverlap;
        private byte[]          _LastDecodedScanline;
        private byte[]          _Unfiltered;
        private byte[]          _CurrentScanline;

        private int             _BytesPerPixel;
        private int             _ScanlineLength;

        private bool            _EOF = false;
        private bool            _disposedValue;
#pragma warning restore IDE0044

        //Properties
        public Image     Image { get { return _Image; } }
        public Physical? Physical { get { return _Physical; } }
        public bool      EOF { get { return _EOF; } }

        public Decoder(Stream inStream, Stream outStream)
        {
            _InputStream = inStream;
            _OutputStream = outStream;

#pragma warning disable CA2208
            if (_InputStream == null) throw new ArgumentNullException(nameof(_InputStream));
            if (!_InputStream.CanRead) throw new ArgumentException("Stream is not readable", nameof(_InputStream));
#pragma warning restore CA2208
            if (_InputStream.Position + 8 > _InputStream.Length) throw new EndOfStreamException();
            //First read the magic word (8 byte);
            if (BitConverter.ToUInt64(_InputStream.Read(8)) != Constants.Magic) throw new Exception("This doesn't seem to be a PNG");

            //Read until the first IDAT chunk is encountered
            _CurrentChunk = new();
            _CurrentChunk.ReadHeader(_InputStream);
            if (_CurrentChunk.Type != "IHDR") throw new Exception("This is not a valid PNG file");
            _Image = new();
            _Image.FromBytes(_CurrentChunk.ReadData(_InputStream));

            do
            {
                _CurrentChunk.ReadHeader(_InputStream);
                if (_CurrentChunk.Type == "pHYs")
                {
                    _Physical = new();
                    _Physical.FromBytes(_CurrentChunk.ReadData(_InputStream));
                }
                else if (_CurrentChunk.Type != "IDAT")
                {
                    _CurrentChunk.SkipData(_InputStream);
                }
            } while (_CurrentChunk.Type != "IDAT");

            _BytesPerPixel = _Image.ColorType.NumBytesPerPixel(_Image.BitDepth);
            _ScanlineLength = _Image.Width * _BytesPerPixel;
            _Unfiltered = new byte[_ScanlineLength];
            _CurrentScanline = new byte[_ScanlineLength];
            _LastDecodedScanline = new byte[_ScanlineLength];
            _LastOverlap = [];

            _ZLibInput = new();
            _ScanlinesUnfiltered = new();
            _ZLibStream = new(_ZLibInput, CompressionMode.Decompress);
            _Temp = new();
        }

        public void ReadScanlines(int numScanlines)
        {
            ArgumentNullException.ThrowIfNull(_Image);
            int expectedLength = _ScanlineLength * numScanlines;
            int expectedLengthWithFilterTypeBytes = (_ScanlineLength + 1) * numScanlines;

            if (_ScanlinesUnfiltered.Length > 0) _ScanlinesUnfiltered.SetLength(0);
            //Write last incomplete buffer into the stream if there is one.
            if (_LastOverlap != null)
            {
                _ScanlinesUnfiltered.Write(_LastOverlap);
                _LastOverlap = [];
            }
            
            //Walk throgh IDAT chunks until either the file ends, we have enough data or the IEND chunk is reached:
            while (true)
            {
                if (_CurrentChunk?.Type == "IEND" || _InputStream.Position >= _InputStream.Length - 4)
                    break;
                if (_CurrentChunk?.Type != "IDAT") throw new Exception("???");
                _ZLibInput.Write(_InputStream.Read(_CurrentChunk.Length));
                _ZLibInput.Position = 0;
                if (_Temp == null || _Temp.Length != _CurrentChunk.Length)
                    _Temp = new(_CurrentChunk.Length);
                else
                    _Temp.Position = 0;
                _ZLibStream.CopyTo(_Temp);
                _ZLibInput.SetLength(0);
                _ScanlinesUnfiltered.Write(_Temp.ToArray());
                _InputStream.Position += 4;
                _CurrentChunk?.ReadHeader(_InputStream);
                if (_ScanlinesUnfiltered.Length >= expectedLengthWithFilterTypeBytes) break;
            }

            if (_CurrentChunk?.Type == "IEND" || _InputStream.Position >= _InputStream.Length - 4)
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
            for (int i = 0; i < expectedLengthWithFilterTypeBytes; i+= _ScanlineLength + 1)
            {
                flt = (Filter)_ScanlinesUnfiltered.ReadByte();
                //if ((byte)flt > 4) throw new Exception("Invalit filter designation");
                _ScanlinesUnfiltered.Read(_CurrentScanline, 0, _ScanlineLength);
                switch (flt)
                {
                    case Filter.None:
                        _OutputStream.Write(_CurrentScanline);
                        _CurrentScanline.CopyTo(_LastDecodedScanline, 0);
                        break;
                    case Filter.Sub:
                        for (int col = 0; col < _CurrentScanline.Length; col++)
                            _Unfiltered[col] = Sub.UnFilter(_CurrentScanline, _Unfiltered, col, _BytesPerPixel);
                        break;
                    case Filter.Up:
                        for (int col = 0; col < _CurrentScanline.Length; col++)
                            _Unfiltered[col] = Up.UnFilter(_CurrentScanline, _LastDecodedScanline, col, _BytesPerPixel);
                        break;
                    case Filter.Average:
                        for (int col = 0; col < _CurrentScanline.Length; col++)
                            _Unfiltered[col] = Average.UnFilter(_CurrentScanline, _Unfiltered, _LastDecodedScanline, col, _BytesPerPixel);
                        break;
                    case Filter.Paeth:
                        for (int col = 0; col < _CurrentScanline.Length; col++)
                            _Unfiltered[col] = Paeth.UnFilter(_CurrentScanline, _Unfiltered, _LastDecodedScanline, col, _BytesPerPixel);
                        break;
                    default:
                        break;
                }
                _Unfiltered.CopyTo(_LastDecodedScanline, 0);
                _OutputStream.Write(_Unfiltered);
            }
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
