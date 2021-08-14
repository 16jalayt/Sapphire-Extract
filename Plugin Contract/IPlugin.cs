using System;
using System.IO;

namespace Plugin_Contract
{
    public interface IPlugin
    {
        public string Name { get;}
        public bool init(Serilog.ILogger masterlogger);
        public bool CanExtract(BinaryReader InStream);
        public bool Extract(BinaryReader InStream);
        public int GetPriority();
    }
}
