using System;
using System.IO.Compression;
using System.IO;

namespace UnPSARC
{
    public static class Zlib
    {
        public static byte[] Decompress(byte[] Data)
        {
            MemoryStream input = new MemoryStream(Data);
            MemoryStream output = new MemoryStream();
            input.Position = 2;
            using (DeflateStream zlib = new DeflateStream(input, CompressionMode.Decompress, false))
            {
                zlib.CopyTo(output);
                zlib.Dispose();
            }

            return output.ToArray();
        }
    }
}
