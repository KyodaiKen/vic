using System.Text;

namespace libvic.FileFormat.Headers
{
    public class MetaDataHeader
    {
        string Type { get; set; }

        MetaDataHeader()
        {
            Type = "";
        }

        public MemoryStream ToMemoryStream()
        {
            MemoryStream tmpMs = new();
            tmpMs.WriteByte((byte)Encoding.UTF8.GetByteCount(Type));
            tmpMs.Write(Encoding.UTF8.GetBytes(Type));
            return tmpMs;
        }

        public void WriteToStream(Stream stream)
        {
            stream.Write(ToMemoryStream().ToArray());
        }

        public void ReadFromStream(Stream stream)
        {
            var len = stream.ReadByte();
            Type = Encoding.UTF8.GetString(stream.Read(len));
        }

        public bool TryReadingFromStream(Stream stream)
        {
            try
            {
                ReadFromStream(stream);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
