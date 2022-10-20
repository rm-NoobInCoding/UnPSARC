using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zlib;
using System.IO;

namespace UnPSARC
{
    public static class Zlib
    {
        public static byte[] ZLibNoMagic = { 0x78, 0x01 };
        public static byte[] ZLibDefaultMagic = { 0x78, 0x9C };
        public static byte[] ZLibBestMagic = { 0x78, 0xDA };
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
