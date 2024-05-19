using Gibbed.IO;
using System;
using System.IO;

namespace UnPSARC
{
    public class TEntry
    {
        private int index;
        public byte[] HashNames;
        public int ZSizeIndex;
        public long UncompressedSize;
        public long Offset;
        public byte[] OverwriteBuffer = { };
        public TEntry(int index) => this.index = index;
        public void Read(Stream Reader)
        {
            HashNames = Reader.ReadBytes(0x10);
            ZSizeIndex = Reader.ReadValueS32(Endian.Big);
            UncompressedSize = ((Int64)Reader.ReadByte()) << 32 | Reader.ReadValueU32(Endian.Big);
            Offset = ((Int64)Reader.ReadByte()) << 32 | Reader.ReadValueU32(Endian.Big);
        }

    }
}
