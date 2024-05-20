using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System;
using System.IO;
using System.IO.Compression;
using UnPSARC.Helpers;

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
            input.Dispose();

            return output.ToArray();
        }
        public static byte[] DecompressZlibStream(byte[] Data)
        {
            var compressedStream = new MemoryStream(Data);
            var decompressedStream = new HugeMemoryStream();
            using (var zlibStream = new InflaterInputStream(compressedStream))
            {
                zlibStream.CopyTo(decompressedStream);
            }
            decompressedStream.Position = 0;

            return decompressedStream.ToByteArray();
        }
    }
}
