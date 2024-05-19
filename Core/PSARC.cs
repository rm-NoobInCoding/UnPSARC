using Gibbed.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UnPSARC
{
    internal class PSARC
    {
        private string ArchiveMagic;
        private ushort MajorVersion;
        private ushort MinorVersion;
        public string CompressionType;
        public int StartOFDatas;
        public int SizeOfEntry;
        public int FilesCount;
        public int ZSizeCount;
        public int BlockSize;
        private int Zero;
        public TEntry[] Entries;
        public TZSize[] ZSizes;
        public Dictionary<string, string> FileNames;
        public Stream Reader;
        public Stream Writer;
        public PSARC(Stream Reader)
        {
            this.Reader = Reader;
        }
        public PSARC(Stream Reader, Stream Writer)
        {
            this.Reader = Reader;
            this.Writer = Writer;
        }
        public void Read()
        {
            Reader.Seek(0, SeekOrigin.Begin);
            ArchiveMagic = Reader.ReadString(4);
            if (ArchiveMagic == "DSAR")
            {
                Console.WriteLine("Archive is Compressed (DSAR), trying to Decompress...");
                Reader = DirectStorage.Decompressor.Decompress(Reader);
                Reader.Seek(0, SeekOrigin.Begin);
                ArchiveMagic = Reader.ReadString(4);
            }

            if (ArchiveMagic != "PSAR") throw new Exception("Not valid PSARC file! Magic:" + ArchiveMagic);
            MajorVersion = Reader.ReadValueU16(Endian.Big);
            MinorVersion = Reader.ReadValueU16(Endian.Big);
            CompressionType = Reader.ReadString(4);
            if (CompressionType != "oodl" && CompressionType != "zlib") throw new Exception("Unsupported Compression method : " + CompressionType);
            if (CompressionType == "oodl" && !Program.oodleExist)
            {
                Console.WriteLine("'oo2core_9_win64.dll' does not exist in the current application path!!!");
                Console.WriteLine("To fix this, copy the dll from your game directory to the same directory that UnPSARC.exe is stored.");
                return;
            }

            StartOFDatas = Reader.ReadValueS32(Endian.Big);
            SizeOfEntry = Reader.ReadValueS32(Endian.Big);
            FilesCount = Reader.ReadValueS32(Endian.Big);
            ZSizeCount = (StartOFDatas - (SizeOfEntry * FilesCount) + 32) / 2;
            BlockSize = Reader.ReadValueS32(Endian.Big);
            Zero = Reader.ReadValueS32(Endian.Big);
            Entries = new TEntry[FilesCount];
            for (int index = 0; index < FilesCount; ++index)
            {
                TEntry tentry = new TEntry(index);
                tentry.Read(Reader);
                Entries[index] = tentry;
            }
            ZSizes = new TZSize[ZSizeCount];
            for (int index = 0; index < ZSizeCount; ++index)
            {
                TZSize tzsize = new TZSize(index);
                tzsize.Read(Reader);
                ZSizes[index] = tzsize;
            }
            FileNames = new Dictionary<string, string>(LoadFileNames(Archive.TryUnpack(Reader, Entries[0], ZSizes, BlockSize, CompressionType)));

        }
        private Dictionary<string, string> LoadFileNames(byte[] file)
        {
            string[] Names = Encoding.UTF8.GetString(file).Split(new[] { "\n", "\0" }, StringSplitOptions.None);
            Dictionary<string, string> ret = new Dictionary<string, string>();
            ret.Add(BitConverter.ToString(new byte[16]), "Filenames.txt");
            foreach (string Name in Names)
            {
                ret.Add(BitConverter.ToString(IOHelper.GetMD5(Name)), Name);
            }
            return ret;
        }

    }
}
