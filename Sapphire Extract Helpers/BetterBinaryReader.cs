
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sapphire_Extract_Helpers
{
    public class BetterBinaryReader
    {
        FileStream fs;
        BinaryReader br;

        public BetterBinaryReader(string fileName)
        {
            fs = new FileStream(fileName, FileMode.Open);
            br = new BinaryReader(fs, Encoding.Default);

            //BR.ReadBytes()
        }

        public void Seek(long pos)
        {
            br.BaseStream.Seek(pos, SeekOrigin.Begin);
        }

        public long Skip(long offset)
        {
            //return current pos
            return br.BaseStream.Seek(offset, SeekOrigin.Current);
        }

        public long Position()
        {
            return br.BaseStream.Position;
        }

        public int ReadInt()
        {
            return br.ReadInt32();
        }

        public byte[] ReadBytes(int len)
        {
            return br.ReadBytes(len);
        }

        public void Dispose()
        {
            br.Dispose();
            fs.Dispose();
        }
    }
}
