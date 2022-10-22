using Gibbed.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace UnPSARC
{
    public class Archive
    {

        public static void Unpack(Stream ArchiveRaw, string Folder)
        {
            Stream Reader = ArchiveRaw;

            //Parse PSARC archive
            PSARC Psarc = new PSARC(Reader);
            Psarc.Read();

            for (int i = 0; i < Psarc.FilesCount; i++)
            {
                //Get information about entry
                TEntry ThisEntry = Psarc.Entries[i];

                //Some Files have 0 size so its better to ignore them
                if (ThisEntry.Offset == 0) continue;
                Console.WriteLine("- Exporting " + Psarc.FileNames[i] + " ...");
                //Trying to unpack the data
                byte[] UnpackedData = TryUnpack(Reader, Psarc.Entries[i], Psarc.ZSizes, Psarc.BlockSize, Psarc.CompressionType);

                //And finally write decompressed data
                IOHelper.CheckFolderExists(Psarc.FileNames[i], Folder);
                File.WriteAllBytes(Path.Combine(Folder, Psarc.FileNames[i]), UnpackedData);
                Console.WriteLine("[" + i + "] " + Psarc.FileNames[i] + " Exported!");

            }
            Console.WriteLine("Unpacking done! | " + Psarc.FilesCount + " File Exported");
            Console.WriteLine("Press Any Key To Continue...");
            Console.ReadKey();

        }

        //just for some tests. game can read loose files so it means we not need a repacker
        public static void Repack(Stream ArchiveRaw, Dictionary<string, byte[]> Assets, string TargetPath)
        {
            Stream Reader = ArchiveRaw;
            Stream Writer = File.Open(TargetPath, FileMode.Create, FileAccess.ReadWrite);
            PSARC Psarc = new PSARC(Reader, Writer);
            Psarc.Read();
            long EndOfReader = Reader.Length;
            Stream MEMORY_FILE = new MemoryStream();
            for (int index = 0; index < Psarc.FilesCount; index++)
            {
                if (Assets.ContainsKey(Psarc.FileNames[index]))
                {
                    Psarc.Entries[index].OverwriteBuffer = Assets[Psarc.FileNames[index]];
                }
                int ZSizeIndex = Psarc.Entries[index].ZSizeIndex;
                List<TZSize> NewZSizeTable = new List<TZSize>();
                long BlockOffset = Psarc.Entries[index].Offset;
                int RemainingSize = Psarc.Entries[index].UncompressedSize;
                while (true)
                {
                    int CompressedSize = Psarc.ZSizes[ZSizeIndex].ZSize;
                    if (CompressedSize == Psarc.Entries[index].UncompressedSize || Psarc.Entries[index].OverwriteBuffer.Length > 0)
                    {

                        MEMORY_FILE.WriteBytes(Reader.ReadAtOffset(BlockOffset, Psarc.Entries[index].UncompressedSize));
                        Psarc.ZSizes[ZSizeIndex].ZSize = Psarc.Entries[index].UncompressedSize;
                        break;

                    }
                    else if (RemainingSize < Psarc.BlockSize || CompressedSize == Psarc.BlockSize || CompressedSize == 0)
                    {

                        MEMORY_FILE.WriteBytes(Reader.ReadAtOffset(BlockOffset, RemainingSize));

                    }
                    else
                    {

                        MEMORY_FILE.WriteBytes(Reader.ReadAtOffset(BlockOffset, Psarc.BlockSize));
                        break;
                    }
                    ZSizeIndex++;
                }


            }

        }
        
        public static byte[] TryUnpack(Stream Reader, TEntry ThisEntry, TZSize[] ZSizes, int BlockSize, string CompressionType)
        {
            int RemainingSize = ThisEntry.UncompressedSize; //This will help us in multi chunked buffers
            int ZSizeIndex = ThisEntry.ZSizeIndex;
            long BlockOffset = ThisEntry.Offset;
            Stream MEMORY_FILE = new MemoryStream();

            //Because we dont have count of blocks (for compressed blocks) we have to use while true until file is fully decompressed
            while (MEMORY_FILE.Length < ThisEntry.UncompressedSize)
            {
                int CompressedSize = ZSizes[ZSizeIndex++].ZSize;
                if (CompressedSize == 0) CompressedSize = BlockSize;
                if (CompressedSize == ThisEntry.UncompressedSize)
                {

                    MEMORY_FILE.WriteBytes(Reader.ReadAtOffset(BlockOffset, ThisEntry.UncompressedSize));

                }
                else if (RemainingSize < BlockSize || CompressedSize == BlockSize)
                {

                    MEMORY_FILE.WriteBytes(Reader.ReadAtOffset(BlockOffset, RemainingSize, CompressedSize, CompressionType));

                }
                else
                {

                    MEMORY_FILE.WriteBytes(Reader.ReadAtOffset(BlockOffset, BlockSize, CompressedSize, CompressionType));

                }
                BlockOffset += (uint)CompressedSize; //Start of next block if needed
                RemainingSize -= BlockSize;
            }
            return MEMORY_FILE.ToByteArray();
        }

    }
}
