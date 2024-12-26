using Gibbed.IO;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace UnPSARC
{
    public static class IOHelper
    {
        public static byte[] ReadAtOffset(this Stream s, long Offset, long Size)
        {
            long pos = s.Position;
            s.Seek(Offset, SeekOrigin.Begin);
            byte[] log = s.ReadBytes((int)Size);
            s.Seek(pos, SeekOrigin.Begin);
            return log;
        }
        public static byte[] ReadAtOffset(this Stream s, long Offset, long size, int ZSize, string CompressionType)
        {

            long pos = s.Position;
            s.Seek(Offset, SeekOrigin.Begin);
            int magic = s.ReadValueU16();
            s.Seek(Offset, SeekOrigin.Begin);
            byte[] Block = s.ReadBytes(ZSize);
            byte[] log = { };
            if (CompressionType == "oodl" && magic == 0x68C) log = Oodle.Decompress(Block, (int)size, Oodle.OodleLZ_FuzzSafe.No, Oodle.OodleLZ_CheckCRC.No, Oodle.OodleLZ_Verbosity.None, Oodle.OodleLZ_Decode_ThreadPhase.Unthreaded);
            if (CompressionType == "zlib" && magic == 0xDA78) log = Zlib.Decompress(Block, (int)size);
            if (log.Length == 0) log = Block;
            s.Seek(pos, SeekOrigin.Begin);
            return log;
        }
        public static byte[] ToByteArray(this Stream a)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                a.Position = 0;
                a.CopyTo(ms);
                return ms.ToArray();
            }
        }
        public static byte[] GetMD5(string input)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                return hashBytes;
            }
        }
        public static string GetMD5String(string input)
        {
            return BitConverter.ToString(GetMD5(input));
        }
        public static void CheckFolderExists(string filename, string basefolder)
        {
        	if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        	{
        		if (!Directory.Exists(Path.GetDirectoryName(@"\\?\" + Path.Combine(basefolder, filename))))
        		{
        			Directory.CreateDirectory(Path.GetDirectoryName(@"\\?\" + Path.Combine(basefolder, filename)));
  	        	}
        	}
        	else
        	{
        		if (!Directory.Exists(Path.GetDirectoryName(Path.Combine(basefolder, filename))))
        	    {
        	    	Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(basefolder, filename)));
        	  	}
        	}
            
        }
    }
}
