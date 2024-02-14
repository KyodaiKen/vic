using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libkuric.FileFormat
{
    internal class MetaDataHeader
    {
        uint    FieldID            { get; set; }
        uint    ParentFieldID      { get; set; }
        byte[]  Type               { get; set; } //Always 8 bytes!

        public MemoryStream ToMemoryStream()
        {
            MemoryStream tmpMs = new MemoryStream();
            tmpMs.Write(BitConverter.GetBytes(FieldID));
            tmpMs.Write(BitConverter.GetBytes(ParentFieldID));
            tmpMs.Write(Type);
            return tmpMs;
        }

        public uint GetHeaderLength()
        {
            return 16;
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
