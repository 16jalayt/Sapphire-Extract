using System.IO;
using System.Text;

namespace Sapphire_Extract_Helpers
{
    public class BetterBinaryReader
    {
        private FileStream _fs;
        private BinaryReader _br;
        private string _fileName;


        public BetterBinaryReader(string fileName)
        {
            _fileName = fileName;
            _fs = new FileStream(fileName, FileMode.Open);
            _br = new BinaryReader(_fs, Encoding.Default);
        }

        public void Seek(long pos)
        {
            _br.BaseStream.Seek(pos, SeekOrigin.Begin);
        }

        public long Skip(long offset)
        {
            //return current pos
            return _br.BaseStream.Seek(offset, SeekOrigin.Current);
        }

        public long Position()
        {
            return _br.BaseStream.Position;
        }

        public string FileName()
        {
            return _fileName;
        }

        public int ReadInt()
        {
            return _br.ReadInt32();
        }

        public byte[] ReadBytes(int len)
        {
            return _br.ReadBytes(len);
        }

        public void Dispose()
        {
            _br.Dispose();
            _fs.Dispose();
        }
    }
}