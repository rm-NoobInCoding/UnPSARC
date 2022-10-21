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

            //Parse PSARC archive
            PSARC Psarc = new PSARC(Reader);
            Psarc.Read();

            for (int i = 0; i < Psarc.FilesCount; i++)
            {
                TEntry ThisEntry = Psarc.Entries[i];
                if (ThisEntry.Offset == 0) continue; //Some Files have 0 size so its better to ignore them
                int RemainingSize = ThisEntry.UncompressedSize; //This will help us in multi chunked buffers
                int ZSizeIndex = ThisEntry.ZSizeIndex;
                long BlockOffset = ThisEntry.Offset;
                Stream MEMORY_FILE = new MemoryStream();

                //Because we dont have count of blocks (for compressed blocks) we have to use while true until file is fully decompressed
                while (true)
                {
                    int CompressedSize = Psarc.ZSizes[ZSizeIndex++].ZSize;

                    if (CompressedSize == ThisEntry.UncompressedSize)
                    {

                        MEMORY_FILE.WriteBytes(Reader.ReadAtOffset(BlockOffset, ThisEntry.UncompressedSize));

                    }
                    else if (RemainingSize < Psarc.BlockSize || CompressedSize == Psarc.BlockSize || CompressedSize == 0)
                    {

                        MEMORY_FILE.WriteBytes(Reader.ReadAtOffset(BlockOffset, RemainingSize, CompressedSize, Psarc.CompressionType));

                    }
                    else
                    {

                        MEMORY_FILE.WriteBytes(Reader.ReadAtOffset(BlockOffset, Psarc.BlockSize, CompressedSize, Psarc.CompressionType));

                    }

                    //If file is equal to uncompressed size it means file is fully decompressed or exported
                    if (MEMORY_FILE.Length == ThisEntry.UncompressedSize)
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
                            break;
                        }
                    }

                    BlockOffset += (uint)CompressedSize; //Start of next block if needed
                    RemainingSize -= Psarc.BlockSize;
                }

            }
        }
        public static void Repack(Stream ArchiveRaw, Dictionary<string, byte[]> Assets, string TargetPath)
        {
            Stream Reader = ArchiveRaw;


        }
    }
}
