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
            byte[] ZLibNoMagic = { 0x78, 0x01 };
            byte[] ZLibDefaultMagic = { 0x78, 0x9C };
            byte[] ZLibBestMagic = { 0x78, 0xDA };
            int OffsetOFTable = 0x20;

            Reader.Seek(0x08, SeekOrigin.Begin);
            string CompressionType = Reader.ReadString(4);
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
                if (UncompressedSize == 0) continue;                     //Some Files have 0 size so better to ignore them!
                Reader.ReadByte();                                       //A Single Byte
                long OFFSET = Reader.ReadValueU32(Endian.Big);
                int ZEntryOffset = (ZSizeIndex * 2) + ZTableOffset;      //Offset Of ZTable Of this Entry
                Stream MEMORY_FILE = new MemoryStream();
                int RemainingSize = UncompressedSize;                    //this will help us in multi chunked buffers
                Reader.Seek(OFFSET, SeekOrigin.Begin);
                string Magic = BitConverter.ToString(Reader.ReadBytes(2));
                //Check if file is compressed or not

                if (Magic == BitConverter.ToString(OodleLzaMagic) || Magic == BitConverter.ToString(ZLibNoMagic) || Magic == BitConverter.ToString(ZLibDefaultMagic) || Magic == BitConverter.ToString(ZLibBestMagic))
                {

                    while (true)
                    {
                        Reader.Seek(ZEntryOffset, SeekOrigin.Begin);
                        int ZSize = Reader.ReadZSize();
                        if (ZSize == 0) ZSize = ChunkSize;
                        if (RemainingSize < ChunkSize || ZSize == ChunkSize)
                        {
                            MEMORY_FILE.WriteBytes(Reader.ReadAtOffset(OFFSET, RemainingSize, ZSize , CompressionType)); //Amount of ZSIZE data remaining in final block of this file

                        }
                        else
                        {
                            MEMORY_FILE.WriteBytes(Reader.ReadAtOffset(OFFSET, ChunkSize, ZSize, CompressionType));
                        }
                        if (MEMORY_FILE.Length == UncompressedSize)
                        {
                            if (i == 0)
                            {
                                FileNames = new List<string>(Encoding.UTF8.GetString(StreamToByteArray(MEMORY_FILE)).Split(new[] { "\n","\0" }, StringSplitOptions.None));
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
                        OFFSET += (uint)ZSize;
                        RemainingSize -= ChunkSize;
                    }
                }
                else
                {
        
                    MEMORY_FILE.WriteBytes(Reader.ReadAtOffset(OFFSET, UncompressedSize));       //File isn't compressed with oodle lza
                    if (i == 0)
                    {
                        FileNames = new List<string>(Encoding.UTF8.GetString(StreamToByteArray(MEMORY_FILE)).Split(new[] { "\n", "\0" }, StringSplitOptions.None));
                    }
                    else
                    {
                        if (!Directory.Exists(Path.GetDirectoryName(Path.Combine(Folder, FileNames[i - 1])))) Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(Folder, FileNames[i - 1])));
                        File.WriteAllBytes(Path.Combine(Folder, FileNames[i - 1]), StreamToByteArray(MEMORY_FILE));
                        Console.WriteLine(FileNames[i - 1] + " Exported...");
                    }

                }
                OffsetOFTable += SiseOfEntry;
            }


        }
        public static int ReadZSize(this Stream Reader)
        {
            return MakeNum(new byte[] { 00, 00, (byte)Reader.ReadByte(), (byte)Reader.ReadByte() });

        }
        public static byte[] ReadAtOffset(this Stream s, long Offset, int Size)
        {
            long pos = s.Position;
            s.Seek(Offset, SeekOrigin.Begin);
            byte[] log = s.ReadBytes(Size);
            s.Seek(pos, SeekOrigin.Begin);
            return log;
        }
        public static byte[] ReadAtOffset(this Stream s, long Offset, int size, int ZSize , string CompressionType)
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
