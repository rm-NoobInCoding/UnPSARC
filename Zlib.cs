using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zlib;
using System.IO;

namespace UnPSARC
{
    public class Zlib
    {
        public static byte[] Decompress(byte[] Data , int decompsize)
        {
            MemoryStream input = new MemoryStream(Data);
            MemoryStream output = new MemoryStream();
            using (ZlibStream zlib = new ZlibStream(input, CompressionMode.Decompress))
            {
                zlib.CopyTo(output);
            }

            return output.ToArray();
        }
    }
}
