using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace UnPSARC
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Archive.Unpack(File.ReadAllBytes(args[0]));
        }
    }
}
