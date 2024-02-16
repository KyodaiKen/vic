using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libkuric
{
    public static class Helpers
    {
        public static byte[] Read(this Stream stream, int length)
        {
            byte[] buff = new byte[length];
            stream.Read(buff, 0, buff.Length);
            return buff;
        }
    }
}
