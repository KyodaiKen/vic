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

        public void FromStream(Stream stream)
        {
            byte[] readBytes(long length)
            {
                byte[] buff = new byte[length];
                stream.Read(buff);
                return buff;
            }

            FieldID = BitConverter.ToUInt32(readBytes(4));
            ParentFieldID = BitConverter.ToUInt32(readBytes(4));
            Type = readBytes(8);
        }

        public bool TryReadingFromStream(Stream stream)
        {
            try
            {
                FromStream(stream);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
