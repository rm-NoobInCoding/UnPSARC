using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Gibbed.IO;

namespace UnPSARC
{
    internal class TEntry
    {
        private int index;
        public byte[] HashNames;
        public int ZSizeIndex;
        public int UncompressedSize;
        public long Offset;
        public TEntry(int index) => this.index = index;
        public void Read(Stream Reader)
        {
            HashNames = Reader.ReadBytes(0x10);
            ZSizeIndex = Reader.ReadValueS32(Endian.Big);
            Reader.ReadByte();
            UncompressedSize = Reader.ReadValueS32(Endian.Big);
            Reader.ReadByte();
            Offset = Reader.ReadValueU32(Endian.Big);
        }

    }
}
