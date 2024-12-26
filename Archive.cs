using Gibbed.IO;
using System;
using System.IO;
using UnPSARC.Helpers;

namespace UnPSARC
{
    public class Archive
    {
        public static string FNameFileHash = BitConverter.ToString(new byte[16]);

        public static void Unpack(Stream ArchiveRaw, string Folder)
        {
            Stream Reader = ArchiveRaw;
            PSARC Psarc = new PSARC(Reader);
            Console.WriteLine("Files count: " + Psarc.FilesCount);
            Psarc.Read();
            int FailedFiles = 0;

            for (int i = 0; i < Psarc.FilesCount; i++)
            {
                TEntry ThisEntry = Psarc.Entries[i];

                string filenameHash = BitConverter.ToString(ThisEntry.HashNames);

                if (ThisEntry.Offset == 0 || filenameHash == FNameFileHash)
                    continue;

                string FileName;
                if (!Psarc.FileNames.ContainsKey(filenameHash))
                {
                    Console.WriteLine("Archive Contains a hash which is not in filenames table: " + filenameHash);
                    FileName = "_Unknowns" + Path.DirectorySeparatorChar.ToString() + filenameHash.Replace("-", "") + ".bin";
                }
                else
                {
                    FileName = Psarc.FileNames[filenameHash].Replace("/", Path.DirectorySeparatorChar.ToString());
                }


                if (FileName.StartsWith(Path.DirectorySeparatorChar.ToString()))
                    FileName = FileName.Remove(0, 1);

                try
                {
                    TryUnpack(Psarc.Reader, out HugeMemoryStream FileWriter, Psarc.Entries[i], Psarc.ZSizes, Psarc.BlockSize, Psarc.CompressionType);
					IOHelper.CheckFolderExists(FileName, Folder);
                    Stream fileHandle = File.Open(Path.Combine(Folder, FileName), FileMode.Create, FileAccess.Write);
                    FileWriter.CopyTo(fileHandle);
                    fileHandle.Close();
                    Console.WriteLine("[" + i + "] " + FileName + " Exported!");

                }
                catch (Exception ex)
                {
                    var _baseforgroud = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[" + i + "] " + FileName + " Cannot Exported! Error:" + ex.Message);
                    Console.ForegroundColor = _baseforgroud;
                    Console.WriteLine("Press any key to continue unpacking");
                    Console.ReadKey();
                    FailedFiles++;
                }

            }

            Console.WriteLine($"Unpacking done! | {Psarc.FilesCount - FailedFiles} of {Psarc.FilesCount} Files Exported");
        }

        public static void TryUnpack(Stream Reader, out HugeMemoryStream Writer, TEntry ThisEntry, TZSize[] ZSizes, int BlockSize, string CompressionType)
        {
            long RemainingSize = ThisEntry.UncompressedSize;
            int ZSizeIndex = ThisEntry.ZSizeIndex;
            long BlockOffset = ThisEntry.Offset;
            Stream MEMORY_FILE = new HugeMemoryStream();

            while (MEMORY_FILE.Length < ThisEntry.UncompressedSize)
            {
                int CompressedSize = ZSizes[ZSizeIndex++].ZSize;

                if (CompressedSize == 0)
                    CompressedSize = BlockSize;

                if (CompressedSize == ThisEntry.UncompressedSize)
                    MEMORY_FILE.WriteBytes(Reader.ReadAtOffset(BlockOffset, ThisEntry.UncompressedSize));
                else if (RemainingSize < BlockSize || CompressedSize == BlockSize)
                    MEMORY_FILE.WriteBytes(Reader.ReadAtOffset(BlockOffset, RemainingSize, CompressedSize, CompressionType));
                else
                    MEMORY_FILE.WriteBytes(Reader.ReadAtOffset(BlockOffset, BlockSize, CompressedSize, CompressionType));

                BlockOffset += (uint)CompressedSize;
                RemainingSize -= BlockSize;
            }

            MEMORY_FILE.Position = 0;
            Writer = (HugeMemoryStream)MEMORY_FILE;
        }

    }
}
