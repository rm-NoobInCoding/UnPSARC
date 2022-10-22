using Gibbed.IO;
using System;
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
            int magic = s.ReadValueU16();
            s.Seek(Offset, SeekOrigin.Begin);
            byte[] Block = s.ReadBytes(ZSize);
            byte[] log = { };
            if (CompressionType == "oodl" && magic == 0x68C) log = Oodle.Decompress(Block, size);
            if (CompressionType == "zlib" && magic == 0xDA78) log = Zlib.Decompress(Block);
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
        public static void CheckFolderExists(string filename, string basefolder)
        {
            if (!Directory.Exists(Path.GetDirectoryName(Path.Combine(basefolder, filename))))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(basefolder, filename)));
            }
        }
    }
}
