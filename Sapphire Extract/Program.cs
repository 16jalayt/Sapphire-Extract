using Sapphire_Extract_Common;
using Serilog;
using Serilog.Events;

namespace Sapphire_Extract
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Core.init();

            //TODO: call cli parse, set logger
            using var log = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Verbose()
#endif
                //.WriteTo.Trace(restrictedToMinimumLevel: LogEventLevel.Debug)
                .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Verbose)
                //.WriteTo.Debug(restrictedToMinimumLevel: LogEventLevel.Debug)
                //.WriteTo.File($"logs\log{timestamp}.txt", restrictedToMinimumLevel: LogEventLevel.Information)
                //.WriteTo.TextWriter(_messages)
                .CreateLogger();
            Serilog.Log.Logger = log;

            foreach (string file in Core.FileList)
            {
                Serilog.Log.Information("Extracting: " + file);
                Core.ExtractFile(file);
            }
        }
    }
}