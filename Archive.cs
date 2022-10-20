using Gibbed.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UnPSARC
{
    public class Archive
    {

        public static void Unpack(Stream ArchiveRaw, string Folder)
        {
            Stream Reader = ArchiveRaw;
            List<string> FileNames = new List<string>();

            int OffsetOFTable = 0x20;

            //Read Header
            string ArchiveMagic = Reader.ReadString(4); //PSARC
            short MajorVersion = Reader.ReadValueS16();
            short MinorVersion = Reader.ReadValueS16();
            string CompressionType = Reader.ReadString(4);
            int StartOFDatas = Reader.ReadValueS32(Endian.Big);
            int SiseOfEntry = Reader.ReadValueS32(Endian.Big);
            int FilesCount = Reader.ReadValueS32(Endian.Big);
            int ChunkSize = Reader.ReadValueS32(Endian.Big);
            int ZTableOffset = (FilesCount * SiseOfEntry) + OffsetOFTable; //ZTable is after Files Entry

            for (int i = 0; i < FilesCount; i++)
            {
                //Read Entry
                Reader.Seek(OffsetOFTable, SeekOrigin.Begin);
                Reader.ReadBytes(0x10); //Maybe Hash Names
                int ZSizeIndex = Reader.ReadValueS32(Endian.Big); //Index Of ZSize In ZSizeTable
                Reader.ReadByte(); //A Single Byte
                int UncompressedSize = Reader.ReadValueS32(Endian.Big);//Real Size of file after decompression
                if (UncompressedSize == 0) continue; //Some Files have 0 size so better to ignore them!
                Reader.ReadByte(); //A Single Byte
                long OFFSET = Reader.ReadValueU32(Endian.Big); //Offset of file bytes
                int ZEntryOffset = (ZSizeIndex * 2) + ZTableOffset; //Offset of ZTable of this entry
                Stream MEMORY_FILE = new MemoryStream(); //just a memory for save decompressed chunks
                int RemainingSize = UncompressedSize; //this will help us in multi chunked buffers

                //Check if file is compressed or not
                if (Reader.CheckIfCompressed(OFFSET)) 
                {

                    while (true)
                    {

                        Reader.Seek(ZEntryOffset, SeekOrigin.Begin);
                        int ZSize = Reader.ReadValueU16(Endian.Big);
                        if (ZSize == 0) ZSize = ChunkSize;
                        if (RemainingSize < ChunkSize || ZSize == ChunkSize)
                        {
                            MEMORY_FILE.WriteBytes(Reader.ReadAtOffset(OFFSET, RemainingSize, ZSize, CompressionType)); //Amount of ZSIZE data remaining in final block of this file

                        }
                        else
                        {
                            MEMORY_FILE.WriteBytes(Reader.ReadAtOffset(OFFSET, ChunkSize, ZSize, CompressionType));
                        }
                        if (MEMORY_FILE.Length == UncompressedSize)
                        {
                            if (i == 0)
                            {
                                FileNames = new List<string>(Encoding.UTF8.GetString(MEMORY_FILE.ToByteArray()).Split(new[] { "\n", "\0" }, StringSplitOptions.None));
                                break;
                            }
                            else
                            {
                                if (!Directory.Exists(Path.GetDirectoryName(Path.Combine(Folder, FileNames[i - 1])))) Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(Folder, FileNames[i - 1])));
                                File.WriteAllBytes(Path.Combine(Folder, FileNames[i - 1]), MEMORY_FILE.ToByteArray());
                                Console.WriteLine("[" + i + "]" + FileNames[i - 1] + " Exported");
                                MEMORY_FILE.Close();
                                break;
                            }
                        }

                        ZEntryOffset += 2; //Offset of next block's ZSIZE value
                        OFFSET += (uint)ZSize; //Start of next block
                        RemainingSize -= ChunkSize;
                    }
                }
                else
                {

                    MEMORY_FILE.WriteBytes(Reader.ReadAtOffset(OFFSET, UncompressedSize)); //File isn't compressed with oodle lza
                    if (i == 0)
                    {
                        FileNames = new List<string>(Encoding.UTF8.GetString(MEMORY_FILE.ToByteArray()).Split(new[] { "\n", "\0" }, StringSplitOptions.None));
                    }
                    else
                    {
                        if (!Directory.Exists(Path.GetDirectoryName(Path.Combine(Folder, FileNames[i - 1])))) Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(Folder, FileNames[i - 1])));
                        File.WriteAllBytes(Path.Combine(Folder, FileNames[i - 1]), MEMORY_FILE.ToByteArray());
                        Console.WriteLine("[" + i + "]" + FileNames[i - 1] + " Exported...");
                    }

                }
                OffsetOFTable += SiseOfEntry;
            }
        }
    }
}
