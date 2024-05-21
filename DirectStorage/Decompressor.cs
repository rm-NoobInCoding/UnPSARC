using Gibbed.IO;
using K4os.Compression.LZ4;
using System.IO;
using UnPSARC.Helpers;

namespace UnPSARC.DirectStorage
{
    internal class Decompressor
    {
        public static Stream Decompress(Stream compressedStream)
        {
            Stream fs = compressedStream;
            fs.Seek(0, SeekOrigin.Begin);
            Stream wt = new HugeMemoryStream();
            fs.ReadBytes(8);
            int count = fs.ReadValueS32();
            fs.ReadBytes(20);

            for (int i = 0; i < count; i++)
            {
                long offsetDec = fs.ReadValueS64();
                long offsetCom = fs.ReadValueS64();
                int sizeDec = fs.ReadValueS32();
                int sizeCom = fs.ReadValueS32();
                int CompType = fs.ReadByte();
                fs.ReadBytes(7);
                if(sizeCom == 0) //padding
                {
                    wt.WriteBytes(new byte[sizeDec]);
                    continue;

                }
                long back_ = fs.Position;

                fs.Seek(offsetCom, SeekOrigin.Begin);

                var buffer = fs.ReadBytes(sizeCom);

                var target = new byte[sizeDec];
                if (CompType == 0)
                    target = buffer;
                else
                    LZ4Codec.Decode(buffer, 0, buffer.Length, target, 0, target.Length);
                
                fs.Seek(back_, SeekOrigin.Begin);
                wt.WriteBytes(target);
            }
            return wt;
        }
    }
}
