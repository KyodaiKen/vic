using System.Text;

namespace libvic.FileFormat
{
    public class MetaDataHeader
    {
        uint    FieldID            { get; set; }
        uint    ParentFieldID      { get; set; }
        string  Type               { get; set; }

        MetaDataHeader()
        {
            Type = "";
        }

        public MemoryStream ToMemoryStream()
        {
            MemoryStream tmpMs = new();
            tmpMs.Write(BitConverter.GetBytes(FieldID));
            tmpMs.Write(BitConverter.GetBytes(ParentFieldID));
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
            FieldID = BitConverter.ToUInt32(stream.Read(4));
            ParentFieldID = BitConverter.ToUInt32(stream.Read(4));
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
