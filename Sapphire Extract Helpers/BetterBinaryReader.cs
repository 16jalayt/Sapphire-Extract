using System.IO;
using System.Text;

namespace Sapphire_Extract_Helpers
{
    public class BetterBinaryReader
    {
        private FileStream _fs;
        private BinaryReader _br;
        private string _fileName;
        private string _filePath;

        public BetterBinaryReader(string filePath)
        {
            _filePath = Path.GetFullPath(@filePath);
            _fileName = Path.GetFileName(@_filePath);
            _fs = new FileStream(@filePath, FileMode.Open);
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

        public string FilePath()
        {
            return _filePath;
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