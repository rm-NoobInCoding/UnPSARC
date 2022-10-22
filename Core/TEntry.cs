using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Gibbed.IO;

namespace UnPSARC
{
    public class TEntry
    {
        private int index;
        public byte[] HashNames;
        public int ZSizeIndex;
        public int UncompressedSize;
        public long Offset;
        public byte[] OverwriteBuffer = { };
        private int Unk1; //maybe always zero?
        private int Unk2;
        public TEntry(int index) => this.index = index;
        public void Read(Stream Reader)
        {
            HashNames = Reader.ReadBytes(0x10);
            ZSizeIndex = Reader.ReadValueS32(Endian.Big);
            Unk1 = Reader.ReadByte();
            UncompressedSize = Reader.ReadValueS32(Endian.Big);
            Unk2 = Reader.ReadByte();
            Offset = Reader.ReadValueU32(Endian.Big);
        }
        public void Write(Stream Writer)
        {
            Writer.WriteBytes(HashNames);
            Writer.WriteValueS32(ZSizeIndex, Endian.Big);
            Writer.WriteByte((byte)Unk1);
            Writer.WriteValueS32(UncompressedSize, Endian.Big);
            Writer.WriteByte((byte)Unk2);
            Writer.WriteValueU32(Convert.ToUInt32(Offset), Endian.Big);
        }

    }
}
