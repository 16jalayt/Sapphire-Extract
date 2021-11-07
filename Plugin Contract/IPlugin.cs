using Sapphire_Extract_Helpers;

namespace Plugin_Contract
{
    public interface IPlugin
    {
        public string Name { get; }
        //public int Priority { get; }

        public bool Init(Serilog.ILogger masterlogger);

        public bool CanExtract(BetterBinaryReader InStream);

        public bool Extract(BetterBinaryReader InStream);

        //public void Cleanup();
    }
}