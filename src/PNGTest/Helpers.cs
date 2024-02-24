using System.Runtime.InteropServices;

namespace PNGTest
{
    static class Helpers
    {
        public static string HumanReadibleSize(long fSize)
        {
            string[] sizes = ["bytes", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB", "YiB"];
            decimal len = fSize;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return String.Format("{0:0.00} {1}", len, sizes[order]);
        }
    }
}
