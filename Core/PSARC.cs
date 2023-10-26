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
            if (ArchiveMagic != "PSAR") throw new Exception("Not valid PSARC file! Magic:" + ArchiveMagic);
            MajorVersion = Reader.ReadValueU16(Endian.Big);
            MinorVersion = Reader.ReadValueU16(Endian.Big);
            CompressionType = Reader.ReadString(4);
            if (CompressionType != "oodl" && CompressionType != "zlib") throw new Exception("Unsupported Compression method.");
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
        public void Write(bool CloseStream)
        {
            Writer.SetLength(0);
            Writer.Seek(0, SeekOrigin.Begin);
            Writer.WriteString(ArchiveMagic);
            Writer.WriteValueU16(MajorVersion, Endian.Big);
            Writer.WriteValueU16(MinorVersion, Endian.Big);
            Writer.WriteString(CompressionType);
            Writer.WriteValueS32((int)Writer.Position + (SizeOfEntry * Entries.Length) + (ZSizes.Length * 2), Endian.Big);
            Writer.WriteValueS32(SizeOfEntry, Endian.Big);
            Writer.WriteValueS32(Entries.Length, Endian.Big);
            Writer.WriteValueS32(BlockSize, Endian.Big);
            Writer.WriteValueS32(Zero, Endian.Big);
            for (int index = 0; index < Entries.Length; ++index)
                Entries[index].Write(Writer);
            for (int index = 0; index < ZSizes.Length; ++index)
                ZSizes[index].Write(Writer);
            if (CloseStream) Writer.Close();



        }

    }
}
