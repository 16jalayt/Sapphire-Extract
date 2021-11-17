using Sapphire_Extract_Common;
using Serilog;
using Serilog.Events;

namespace Sapphire_Extract
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //TODO: use core.options.verbose
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

            Core.init();

            foreach (string file in Core.FileList)
            {
                Log.Information("Extracting: " + file);
                Core.ExtractFile(file);
            }
        }
    }
}