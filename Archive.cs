using Gibbed.IO;
using OodleSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UnPSARC
{
    public static class Archive
    {
        public static void Unpack(Stream ArchiveRaw, string Folder)
        {
            Stream Reader = ArchiveRaw;
            List<string> FileNames = new List<string>();
            byte[] OodleLzaMagic = { 0x8C, 0x06 };
            int OffsetOFTable = 0x20;
            Reader.Seek(0x0c, SeekOrigin.Begin);
            int StartOFDatas = Reader.ReadValueS32(Endian.Big);
            int SiseOfEntry = Reader.ReadValueS32(Endian.Big);
            int FilesCount = Reader.ReadValueS32(Endian.Big);
            int ChunkSize = Reader.ReadValueS32(Endian.Big);
            int ZTableOffset = (FilesCount * SiseOfEntry) + OffsetOFTable; //ZTable is after Files Entry

            for (int i = 0; i < FilesCount; i++)
            {
                Reader.Seek(OffsetOFTable, SeekOrigin.Begin);
                Reader.ReadBytes(0x10);                                  //Maybe Hash Names
                int ZSizeIndex = Reader.ReadValueS32(Endian.Big);        //Index Of ZSize In ZSizeTable
                Reader.ReadByte();                                       //A Single Byte
                int UncompressedSize = Reader.ReadValueS32(Endian.Big);  //Real Size of file after decompression
                Reader.ReadByte();                                       //A Single Byte
                int OFFSET = (int)Reader.ReadValueU32(Endian.Big);
                int ZEntryOffset = (ZSizeIndex * 2) + ZTableOffset;      //Offset Of ZTable Of this Entry
                Stream MEMORY_FILE = new MemoryStream();
                int RemainingSize = UncompressedSize;                    //this will help us in multi chunked buffers
                Reader.Seek(OFFSET, SeekOrigin.Begin);

                //Check if file is compressed or not
                if (BitConverter.ToString(Reader.ReadBytes(2)) != BitConverter.ToString(OodleLzaMagic))
                {
                    MEMORY_FILE.WriteBytes(Reader.ReadAtOffset(OFFSET, UncompressedSize));       //File isn't compressed with oodle lza
                    if (i == 0)
                    {
                        FileNames = new List<string>(Encoding.UTF8.GetString(StreamToByteArray(MEMORY_FILE)).Split(new[] { "\n" }, StringSplitOptions.None));
                    }
                    else
                    {
                        if (!Directory.Exists(Path.GetDirectoryName(Path.Combine(Folder, FileNames[i - 1])))) Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(Folder, FileNames[i - 1])));
                        File.WriteAllBytes(Path.Combine(Folder, FileNames[i - 1]), StreamToByteArray(MEMORY_FILE));
                        Console.WriteLine(FileNames[i - 1] + " Exported...");
                    }

                }
                else
                {

                    while (true)
                    {
                        Reader.Seek(ZEntryOffset, SeekOrigin.Begin);
                        int ZSize = Reader.ReadZSize();
                        if (ZSize == 0) ZSize = ChunkSize;
                        if (RemainingSize <= ChunkSize || ZSize == ChunkSize)
                        {
                            MEMORY_FILE.WriteBytes(Reader.ReadAtOffset(OFFSET, RemainingSize, ZSize)); //Amount of ZSIZE data remaining in final block of this file

                        }
                        else
                        {
                            MEMORY_FILE.WriteBytes(Reader.ReadAtOffset(OFFSET, ChunkSize, ZSize));
                        }
                        if (MEMORY_FILE.Length == UncompressedSize)
                        {
                            if (i == 0)
                            {
                                FileNames = new List<string>(Encoding.UTF8.GetString(StreamToByteArray(MEMORY_FILE)).Split(new[] { "\n" }, StringSplitOptions.None));
                                break;
                            }
                            else
                            {
                                if (!Directory.Exists(Path.GetDirectoryName(Path.Combine(Folder, FileNames[i - 1])))) Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(Folder, FileNames[i - 1])));
                                File.WriteAllBytes(Path.Combine(Folder, FileNames[i - 1]), StreamToByteArray(MEMORY_FILE));
                                Console.WriteLine(FileNames[i - 1] + " Exported...");
                                MEMORY_FILE.Close();
                                break;
                            }
                        }
                        ZEntryOffset += 2;
                        OFFSET += ZSize;
                        RemainingSize -= ChunkSize;
                    }
                }
                OffsetOFTable += SiseOfEntry;
            }


        }
        public static int ReadZSize(this Stream Reader)
        {
            return MakeNum(new byte[] { 00, 00, (byte)Reader.ReadByte(), (byte)Reader.ReadByte() });

        }
        public static byte[] ReadAtOffset(this Stream s, int Offset, int Size)
        {
            long pos = s.Position;
            s.Seek(Offset, SeekOrigin.Begin);
            byte[] log = s.ReadBytes(Size);
            s.Seek(pos, SeekOrigin.Begin);
            return log;
        }
        public static byte[] ReadAtOffset(this Stream s, int Offset, int size, int ZSize)
        {
            long pos = s.Position;
            s.Seek(Offset, SeekOrigin.Begin);
            byte[] Block = s.ReadBytes(ZSize);
            byte[] log = Oodle.Decompress(Block, size);
            if (log.Length == 0) log = Block;
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
