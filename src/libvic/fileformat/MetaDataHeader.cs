using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libkuric.FileFormat
{
    public class MetaDataHeader
    {
        uint    FieldID            { get; set; }
        uint    ParentFieldID      { get; set; }
        byte[]  Type               { get; set; } //Always 8 bytes!

        MetaDataHeader()
        {
            Type = [];
        }

        public MemoryStream ToMemoryStream()
        {
            MemoryStream tmpMs = new();
            tmpMs.Write(BitConverter.GetBytes(FieldID));
            tmpMs.Write(BitConverter.GetBytes(ParentFieldID));
            tmpMs.Write(Type);
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
            Type = stream.Read(8);
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
