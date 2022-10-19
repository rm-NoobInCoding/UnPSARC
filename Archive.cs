using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gibbed.IO;
using System.IO;
using OodleSharp;

namespace UnPSARC
{
    public static class Archive
    {
        public static void Unpack(byte[] ArchiveRaw)
        {
            Stream Reader = new MemoryStream(ArchiveRaw);
            List<string> FileNames = new List<string>();
            int TABLE = 0x20;
            Reader.Seek(0x0c,SeekOrigin.Begin);
            int DATA_START = Reader.ReadValueS32(Endian.Big);
            int ENTRY_SIZE = Reader.ReadValueS32(Endian.Big);
            int FILES = Reader.ReadValueS32(Endian.Big);
            int BLOCK_SIZE = Reader.ReadValueS32(Endian.Big);
            int ZTableOffset = (FILES * ENTRY_SIZE) + TABLE;
            for (int i = 0; i < FILES; i++)
            {
                Reader.Seek(TABLE, SeekOrigin.Begin);
                //Console.WriteLine(Reader.Position);
                byte[] Junk = Reader.ReadBytes(0x10);
                int START_BLOCK = Reader.ReadValueS32(Endian.Big);
                
                Reader.ReadByte();
                int TOTAL_SIZE = Reader.ReadValueS32(Endian.Big);
                Reader.ReadByte();
                int OFFSET = Reader.ReadValueS32(Endian.Big);
                int ZEntryOffset = (START_BLOCK * 2) + ZTableOffset;
                Stream MEMORY_FILE = new MemoryStream();
                int TEMP2 = TOTAL_SIZE;
                Reader.Seek(OFFSET, SeekOrigin.Begin);
                byte[] MISC1 = Reader.ReadBytes(2);
                if (BitConverter.ToString(MISC1) != "8C-06")
                {
                    Log(Reader, OFFSET, TOTAL_SIZE);
                }
                else
                {
                    while (true)
                    {
                        Reader.Seek(ZEntryOffset, SeekOrigin.Begin);
                        int ZSize = MakeNum(new byte[] { 00 , 00 , (byte)Reader.ReadByte() , (byte)Reader.ReadByte() });
                        if (TEMP2 < BLOCK_SIZE)
                        {
                            MEMORY_FILE.WriteBytes(CLog(Reader, OFFSET, ZSize, TEMP2));
                        }
                        else
                        {
                            MEMORY_FILE.WriteBytes(CLog(Reader, OFFSET, ZSize, BLOCK_SIZE));
                        }
                        if (MEMORY_FILE.Length == TOTAL_SIZE)
                        {
                            File.WriteAllBytes(i + ".dat", StreamToByteArray(MEMORY_FILE));

                            MEMORY_FILE.SetLength(0);
                            break;
                        }
                        ZEntryOffset += 2;
                        OFFSET += ZSize;
                        TEMP2 -= BLOCK_SIZE;
                    }
                }
                TABLE += ENTRY_SIZE;
            }

        }
        public static byte[] Log(Stream s, int Offset , int Size)
        {
            long pos = s.Position;
            s.Seek(Offset, SeekOrigin.Begin);
            byte[] log = s.ReadBytes(Size);
            s.Seek(pos, SeekOrigin.Begin);
            return log;
        }
        public static byte[] CLog(this Stream s, int Offset, int ZSize , int size)
        {
            long pos = s.Position;
            s.Seek(Offset, SeekOrigin.Begin);
            byte[] log = Oodle.Decompress(s.ReadBytes(ZSize), size);
            s.Seek(pos, SeekOrigin.Begin);
            return log;
        }
        public static byte[] StreamToByteArray(Stream a)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                a.Position = 0;
                a.CopyTo(ms);
                return ms.ToArray();
            }
        }
        public static int MakeNum(byte[] a)
        {

            Array.Reverse(a);
            return BitConverter.ToInt32(a, 0);
        }
    }
}
