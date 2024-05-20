using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System.IO;
using UnPSARC.Helpers;

namespace UnPSARC
{
    public static class Zlib
    {
        public static byte[] Decompress(byte[] Data)
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
