using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace UnPSARC
{

    public static class Oodle
    {
        public enum OodleLZ_Verbosity
        {
            None,
            Max = 3
        }
        public enum OodleLZ_FuzzSafe
        {
            No,
            Yes
        }
        public enum OodleLZ_Decode_ThreadPhase
        {
            ThreadPhase1 = 0x1,
            ThreadPhase2 = 0x2,

            Unthreaded = ThreadPhase1 | ThreadPhase2
        }
        public enum OodleLZ_CheckCRC
        {
            No,
            Yes
        }
        public enum OodleLZ_OodleCompressionLevel
        {
            HyperFast4 = -4,
            HyperFast3,
            HyperFast2,
            HyperFast1,
            None,
            SuperFast,
            VeryFast,
            Fast,
            Normal,
            Optimal1,
            Optimal2,
            Optimal3,
            Optimal4,
            Optimal5,
            // TooHigh,

            Min = HyperFast4,
            Max = Optimal5
        }
        public enum OodleLZ_OodleFormat
        {
            Invalid = -1,
            LZH,
            LZHLW,
            LZNIB,
            None,
            LZB16,
            LZBLW,
            LZA,
            LZNA,
            Kraken,
            Mermaid,
            BitKnit,
            Selkie,
            Hydra,
            Leviathan
        }

        [DllImport("oo2core_9_win64.dll")]
        private static extern long OodleLZ_Decompress(byte[] buffer, long bufferSize, byte[] outputBuffer, long outputBufferSize, OodleLZ_FuzzSafe fuzz, OodleLZ_CheckCRC crc, OodleLZ_Verbosity verbosity, long context, long e, long callback, long callback_ctx, long scratch, long scratch_size, OodleLZ_Decode_ThreadPhase thread_phase);
        public static byte[] Decompress(byte[] buffer, int uncompressedSize, OodleLZ_FuzzSafe fuzz, OodleLZ_CheckCRC crc, OodleLZ_Verbosity verbos, OodleLZ_Decode_ThreadPhase threadPhase)
        {
            byte[] numArray = new byte[uncompressedSize];
            long count = OodleLZ_Decompress(buffer, buffer.Length, numArray, (long)uncompressedSize, fuzz, crc, verbos, 0U, 0U, 0U, 0U, 0U, 0U, threadPhase);
            if (count == uncompressedSize)
                return numArray;
            return count < uncompressedSize ? numArray.Take((int)count).ToArray<byte>() : throw new Exception("There was an error while decompressing");
        }
    }
}