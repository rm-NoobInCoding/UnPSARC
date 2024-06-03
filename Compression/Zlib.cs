using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System.IO;
using UnPSARC.Helpers;

namespace UnPSARC
{
    public static class Zlib
    {
        public static byte[] Decompress(byte[] Data,int decompressedSize)
        {
            var compressedStream = new MemoryStream(Data);
            var decompressedStream = new HugeMemoryStream();
            try
            {
                using (var zlibStream = new InflaterInputStream(compressedStream))
                {
                    zlibStream.CopyTo(decompressedStream);
                }
                decompressedStream.Position = 0;

            }
            catch
            {
                return new byte[decompressedSize];
            }
                
            return decompressedStream.ToByteArray();
        }
    }
}