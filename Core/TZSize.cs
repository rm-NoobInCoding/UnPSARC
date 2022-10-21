using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gibbed.IO;
using System.Text;
using System.Threading.Tasks;

namespace UnPSARC
{
    internal class TZSize
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
