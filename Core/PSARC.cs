using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Gibbed.IO;

namespace UnPSARC
{
    internal class PSARC
    {
        private string ArchiveMagic; //PSARC
        private ushort MajorVersion;
        private ushort MinorVersion;
        public string CompressionType; //oodle Or zlib
        public int StartOFDatas;
        public int SiseOfEntry;
        public int FilesCount;
        public int ZSizeCount;
        public int BlockSize;
        public TEntry[] Entries;
        public TZSize[] ZSizes;
        List<string> FileNames = new List<string>();
        public Stream Reader;
        public Stream Writer;
        public PSARC(Stream Reader)
        {
            this.Reader = Reader;
        }
        public PSARC(Stream Reader,Stream Writer)
        {
            
        }
        public void Read()
        {
            Reader.Seek(0, SeekOrigin.Begin);
            ArchiveMagic = Reader.ReadString(4); //PSAR
            if (ArchiveMagic != "PSAR") throw new Exception("Not valid PSARC file! Magic:" + ArchiveMagic);
            MajorVersion = Reader.ReadValueU16(Endian.Big);
            MinorVersion = Reader.ReadValueU16(Endian.Big);
            CompressionType = Reader.ReadString(4); //oodle Or zlib
            if (CompressionType != "oodl" && CompressionType != "zlib") throw new Exception("Unsupported Compression method.");
            StartOFDatas = Reader.ReadValueS32(Endian.Big);
            SiseOfEntry = Reader.ReadValueS32(Endian.Big);
            FilesCount = Reader.ReadValueS32(Endian.Big);
            ZSizeCount = (StartOFDatas - (SiseOfEntry * FilesCount) + 32) / 2;
            BlockSize = Reader.ReadValueS32(Endian.Big);
            Reader.ReadValueS32(Endian.Big); //Always Zero
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

        }

    }
}
