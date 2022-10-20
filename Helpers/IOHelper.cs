using Gibbed.IO;
using System.IO;
using System.Linq;

namespace UnPSARC
{
    public static class IOHelper
    {
        public static byte[] ReadAtOffset(this Stream s, long Offset, int Size)
        {
            long pos = s.Position;
            s.Seek(Offset, SeekOrigin.Begin);
            byte[] log = s.ReadBytes(Size);
            s.Seek(pos, SeekOrigin.Begin);
            return log;
        }
        public static byte[] ReadAtOffset(this Stream s, long Offset, int size, int ZSize, string CompressionType)
        {
            long pos = s.Position;
            s.Seek(Offset, SeekOrigin.Begin);
            byte[] Block = s.ReadBytes(ZSize);
            byte[] log = { };
            if (CompressionType == "oodl") log = Oodle.Decompress(Block, size);
            if (CompressionType == "zlib") log = Zlib.Decompress(Block, size);
            if (log.Length == 0) log = Block;
            s.Seek(pos, SeekOrigin.Begin);
            return log;
        }
        public static byte[] ToByteArray(this Stream a)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                a.Position = 0;
                a.CopyTo(ms);
                return ms.ToArray();
            }
        }
        public static bool CheckIfCompressed(this Stream Reader, long offset)
        {
            long pos = Reader.Position;
            Reader.Seek(offset, SeekOrigin.Begin);
            byte[] Magic = Reader.ReadBytes(2);
            Reader.Seek(pos, SeekOrigin.Begin);
            if (Magic.SequenceEqual(Oodle.OodleLzaMagic) || Magic.SequenceEqual(Zlib.ZLibNoMagic) || Magic.SequenceEqual(Zlib.ZLibDefaultMagic) || Magic.SequenceEqual(Zlib.ZLibBestMagic))
            {
                return true;
            }
            return false;

        }
    }
}
