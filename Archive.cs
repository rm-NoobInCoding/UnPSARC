using Gibbed.IO;
using System;
using System.IO;

namespace UnPSARC
{
    public class Archive
    {

        public static void Unpack(Stream ArchiveRaw, string Folder)
        {
            Stream Reader = ArchiveRaw;
            PSARC Psarc = new PSARC(Reader);
            Psarc.Read();

            for (int i = 0; i < Psarc.FilesCount; i++)
            {
                TEntry ThisEntry = Psarc.Entries[i];
                if (ThisEntry.Offset == 0) continue;

                if (!Psarc.FileNames.ContainsKey(BitConverter.ToString(ThisEntry.HashNames))) throw new Exception("Archive Contains a hash which is not in filenames table:");
                string FileName = Psarc.FileNames[BitConverter.ToString(ThisEntry.HashNames)].Replace("/", "\\");
                if (FileName.StartsWith("\\")) FileName = FileName.Remove(0, 1);
                byte[] UnpackedData = TryUnpack(Psarc.Reader, Psarc.Entries[i], Psarc.ZSizes, Psarc.BlockSize, Psarc.CompressionType);
                IOHelper.CheckFolderExists(FileName, Folder);
                File.WriteAllBytes(@"\\?\" + Path.Combine(Folder, FileName), UnpackedData);
                Console.WriteLine("[" + i + "] " + FileName + " Exported!");

            }
            Console.WriteLine("Unpacking done! | " + Psarc.FilesCount + " File Exported");
            //Console.WriteLine("Press Any Key To Continue...");
            //Console.ReadKey();

        }

        public static byte[] TryUnpack(Stream Reader, TEntry ThisEntry, TZSize[] ZSizes, int BlockSize, string CompressionType)
        {
            long RemainingSize = ThisEntry.UncompressedSize;
            int ZSizeIndex = ThisEntry.ZSizeIndex;
            long BlockOffset = ThisEntry.Offset;
            Stream MEMORY_FILE = new MemoryStream();

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
            return MEMORY_FILE.ToByteArray();
        }

    }
}
