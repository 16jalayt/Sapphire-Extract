using Sapphire_Extract_Common;
using Serilog;
using Serilog.Events;
using System.IO;
using System.Windows;

namespace Sapphire_Extract_GUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static StringWriter _messages { get; private set; }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            using var log = new LoggerConfiguration()
                //.WriteTo.Trace(restrictedToMinimumLevel: LogEventLevel.Debug)
                //.WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Verbose)
                .WriteTo.Debug(restrictedToMinimumLevel: LogEventLevel.Verbose)
                //.WriteTo.File($"logs\log{timestamp}.txt", restrictedToMinimumLevel: LogEventLevel.Information)
                .WriteTo.TextWriter(_messages)
                .CreateLogger();
            Serilog.Log.Logger = log;
            //Serilog.Log.Information("Hello World!");

            Core.init();
        }
    }
}