using Gibbed.IO;
using System.IO;

namespace UnPSARC
{
    public class TZSize
    {
        private int index;
        public int ZSize;
        public TZSize(int index) => this.index = index;
        public void Read(Stream Reader)
        {
            ZSize = Reader.ReadValueU16(Endian.Big);
        }
    }
}
