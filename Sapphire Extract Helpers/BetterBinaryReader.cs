using Serilog;
using System;
using System.IO;
using System.Text;

namespace Sapphire_Extract_Helpers
{
    public class BetterBinaryReader
    {
        private Stream _s;
        private BinaryReader _br;

        //file.ext
        public string FileName { get; }

        //file
        public string FileNameWithoutExtension { get; }

        //.ext
        public string FileExtension { get; }

        //C:\\folder\\file.ext
        public string FilePath { get; }

        //C:\\folder\\
        public string FileDirectory { get; }

        public BetterBinaryReader(string filePath)
        {
            FilePath = Path.GetFullPath(@filePath);
            FileNameWithoutExtension = Path.GetFileNameWithoutExtension(FilePath);
            FileExtension = Path.GetExtension(FilePath);
            FileName = Path.GetFileName(FilePath);
            FileDirectory = Path.GetDirectoryName(FilePath);
            _s = new FileStream(@filePath, FileMode.Open);
            _br = new BinaryReader(_s, Encoding.Default);
        }

        public BetterBinaryReader(byte[] fileContent, string filePath)
        {
            if (filePath != null && filePath != string.Empty)
            {
                FilePath = Path.GetFullPath(@filePath);
                FileNameWithoutExtension = Path.GetFileNameWithoutExtension(FilePath);
                FileExtension = Path.GetExtension(FilePath);
                FileName = Path.GetFileName(FilePath);
                FileDirectory = Path.GetDirectoryName(FilePath);
            }

            _s = new MemoryStream(fileContent);
            _br = new BinaryReader(_s, Encoding.Default);
        }

        public BetterBinaryReader(byte[] fileContent, BetterBinaryReader copyFrom)
        {
            if (copyFrom != null)
            {
                //Copy parameters from another stream
                FilePath = copyFrom.FilePath;
                FileNameWithoutExtension = copyFrom.FileNameWithoutExtension;
                FileExtension = copyFrom.FileExtension;
                FileName = copyFrom.FileName;
                FileDirectory = copyFrom.FileDirectory;
            }

            _s = new MemoryStream(fileContent);
            _br = new BinaryReader(_s, Encoding.Default);
        }

        public BetterBinaryReader(Stream fileStream, BetterBinaryReader copyFrom)
        {
            if (copyFrom != null)
            {
                //Copy parameters from another stream
                FilePath = copyFrom.FilePath;
                FileNameWithoutExtension = copyFrom.FileNameWithoutExtension;
                FileExtension = copyFrom.FileExtension;
                FileName = copyFrom.FileName;
                FileDirectory = copyFrom.FileDirectory;
            }

            _s = new MemoryStream();
            fileStream.CopyTo(_s);
            _br = new BinaryReader(_s, Encoding.Default);
        }

        public void Seek(long pos)
        {
            _br.BaseStream.Seek(pos, SeekOrigin.Begin);
        }

        public void Seek(long pos, SeekOrigin origin)
        {
            _br.BaseStream.Seek(pos, origin);
        }

        public long Length()
        {
            return _br.BaseStream.Length;
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

        public int ReadInt(string msg = "")
        {
            int data = _br.ReadInt32();
            print(msg, data.ToString());
            return data;
        }

        public uint ReadUInt(string msg = "")
        {
            uint data = _br.ReadUInt32();
            print(msg, data.ToString());
            return data;
        }

        public short ReadShort(string msg = "")
        {
            short data = _br.ReadInt16();
            print(msg, data.ToString());
            return data;
        }

        public ushort ReadUShort(string msg = "")
        {
            ushort data = _br.ReadUInt16();
            print(msg, data.ToString());
            return data;
        }

        public byte[] ReadBytes(int len, string msg = "")
        {
            byte[] data = _br.ReadBytes(len);
            print(msg, data.ToString());
            return data;
        }

        public byte ReadByte(string msg = "")
        {
            byte data = _br.ReadByte();
            print(msg, data.ToString());
            return data;
        }

        //Big Endian
        public int ReadIntBE(string msg = "")
        {
            byte[] data = ReadBytes(4);
            Array.Reverse(data);
            int dataout = BitConverter.ToInt32(data, 0);
            print(msg, dataout.ToString());
            return dataout;
        }

        public short ReadShortBE(string msg = "")
        {
            byte[] data = ReadBytes(2);
            Array.Reverse(data);
            short dataout = BitConverter.ToInt16(data, 0);
            print(msg, dataout.ToString());
            return dataout;
        }

        public uint ReadUIntBE(string msg = "")
        {
            byte[] data = ReadBytes(4);
            Array.Reverse(data);
            uint dataout = BitConverter.ToUInt32(data, 0);
            print(msg, dataout.ToString());
            return dataout;
        }

        public ushort ReadUShortBE(string msg = "")
        {
            byte[] data = ReadBytes(2);
            Array.Reverse(data);
            ushort dataout = BitConverter.ToUInt16(data, 0);
            print(msg, dataout.ToString());
            return dataout;
        }

        public string ReadNullTerminatedString()
        {
            StringBuilder builder = new StringBuilder();
            byte c = 1;
            while (c != 0)
            {
                c = ReadByte();
                builder.Append((char)c);
            }
            string CurrFileName = builder.ToString();
            return CurrFileName.Substring(0, CurrFileName.Length - 1);
        }

        public bool IsEOF()
        {
            return this.Position() >= this.Length() ? true : false;
        }

        public Stream GetStream()
        {
            return _br.BaseStream;
        }

        public short ReadShort()
        {
            return _br.ReadInt16();
        }

        private void print(string msg, string data)
        {
            if (msg.Length != 0)
                Log.Debug(msg + data);
        }

        public void Dispose()
        {
            _br.Dispose();
            _s.Dispose();
        }
    }
}