using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace UnPSARC
{

    public static class Oodle
    {

        [DllImport("oo2core_9_win64.dll")]
        private static extern int OodleLZ_Decompress(byte[] buffer, long bufferSize, byte[] outputBuffer, long outputBufferSize, uint a, uint b, ulong c, uint d, uint e, uint f, uint g, uint h, uint i, uint threadModule);

        public static byte[] Decompress(byte[] buffer, int uncompressedSize)
        {
            byte[] decompressedBuffer = new byte[uncompressedSize];

            int decompressedCount = OodleLZ_Decompress(buffer, buffer.Length, decompressedBuffer, uncompressedSize, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3);
            if (decompressedCount == uncompressedSize)
                return decompressedBuffer;
            else if (decompressedCount < uncompressedSize)
                return decompressedBuffer.Take(decompressedCount).ToArray();
            else
                throw new Exception("There was an error while decompressing");
        }
    }
}