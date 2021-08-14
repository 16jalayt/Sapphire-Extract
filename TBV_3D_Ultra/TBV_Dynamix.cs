using Plugin_Contract;
using Sapphire_Extract_Common;
using Serilog;
using System;
using System.IO;
using System.Linq;

namespace TBV_Dynamix
{
    internal class TBV_Dynamix : IPlugin
    {
        public string Name { get { return "3d Ultra"; } }

        public bool init(Serilog.ILogger masterlogger)
        {
            Serilog.Log.Logger = new Serilog.LoggerConfiguration()
          .MinimumLevel.Debug()
          .WriteTo.Logger(masterlogger)
          .CreateLogger();

            return true;
        }

        public bool CanExtract(BinaryReader InStream)
        {
            //TODO: create helper
            InStream.BaseStream.Seek(33, SeekOrigin.Begin);
            byte[] magic = InStream.ReadBytes(8);

            //If the file has wrong id, say we can't extract
            if (System.Text.Encoding.UTF8.GetString(magic) == "TBVolume")
                //if (magic.SequenceEqual(Encoding.UTF8.GetBytes("TBVolume")))
                return true;
            else
                return false;
        }

        public bool Extract(BinaryReader InStream)
        {
            //seek past magic (TBVolume)
            InStream.BaseStream.Seek(9, SeekOrigin.Begin);
            return true;
        }

        public int GetPriority()
        {
            //TODO:simplify with enum?
            return 100;
        }
    }
}
