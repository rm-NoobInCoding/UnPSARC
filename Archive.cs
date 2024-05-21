using Gibbed.IO;
using System;
using System.IO;
using UnPSARC.Helpers;

namespace UnPSARC
{
    public class Archive
    {

        public static void Unpack(Stream ArchiveRaw, string Folder)
        {
            Stream Reader = ArchiveRaw;
            PSARC Psarc = new PSARC(Reader);
            Psarc.Read();
            int FailedFiles = 0;

            for (int i = 0; i < Psarc.FilesCount; i++)
            {
                TEntry ThisEntry = Psarc.Entries[i];
                if (ThisEntry.Offset == 0) continue;

                if (!Psarc.FileNames.ContainsKey(BitConverter.ToString(ThisEntry.HashNames))) throw new Exception("Archive Contains a hash which is not in filenames table:");
                string FileName = Psarc.FileNames[BitConverter.ToString(ThisEntry.HashNames)].Replace("/", "\\");
                if (FileName.StartsWith("\\")) FileName = FileName.Remove(0, 1);
                try
                {
                    IOHelper.CheckFolderExists(FileName, Folder);
                    TryUnpack(Psarc.Reader, out HugeMemoryStream FileWriter, Psarc.Entries[i], Psarc.ZSizes, Psarc.BlockSize, Psarc.CompressionType);
                    Stream fileHandle = File.Open(@"\\?\" + Path.Combine(Folder, FileName), FileMode.Create, FileAccess.Write);
                    FileWriter.CopyTo(fileHandle);
                    fileHandle.Close();
                    Console.WriteLine("[" + i + "] " + FileName + " Exported!");

                }
                catch (Exception ex)
                {
                    Console.WriteLine("[" + i + "] ---" + FileName + " Cannot Exported! Error:" + ex.Message);
                    Console.WriteLine("Press any key to continue unpacking");
                    Console.ReadLine();
                    FailedFiles++;
                }



            }
            Console.WriteLine($"Unpacking done! | {Psarc.FilesCount - FailedFiles} of {Psarc.FilesCount} Files Exported");

        }

        public static void TryUnpack(Stream Reader,out HugeMemoryStream Writer, TEntry ThisEntry, TZSize[] ZSizes, int BlockSize, string CompressionType)
        {
            long RemainingSize = ThisEntry.UncompressedSize;
            int ZSizeIndex = ThisEntry.ZSizeIndex;
            long BlockOffset = ThisEntry.Offset;
            Stream MEMORY_FILE = new HugeMemoryStream();

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
                BlockOffset += (uint)CompressedSize;
                RemainingSize -= BlockSize;
            }
            MEMORY_FILE.Position = 0;
            Writer = (HugeMemoryStream)MEMORY_FILE;
        }

    }
}
