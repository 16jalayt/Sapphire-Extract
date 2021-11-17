using CommandLine;
using Sapphire_Extract_Common;
using Sapphire_Extract_Helpers;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;

namespace Sapphire_Extract
{
    internal class Program
    {
        //fileList technically private, as is an array, and plugin operates on single file in parameter
        public static List<string> FileList { get; set; } = new List<string>();

        public static bool Verbose { get; set; }
        //public static bool OverwriteAll { get; set; }
        //public static bool AutoRename { get; set; }
        //public static bool Raw { get; set; }

        private static void Main(string[] args)
        {
            //Parse cli
            Parser.Default.ParseArguments<CommandLineOptions>(Environment.GetCommandLineArgs()).MapResult(
                options => ParseSuccess(options), err => ParseFail(err));

            //TODO: use core.options.verbose
            var log = new LoggerConfiguration();
            if (Verbose)
            {
                log.MinimumLevel.Verbose();
                log.WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Verbose);
                //.WriteTo.Trace(restrictedToMinimumLevel: LogEventLevel.Debug)
                //.WriteTo.Debug(restrictedToMinimumLevel: LogEventLevel.Debug)
                //.WriteTo.File($"logs\log{timestamp}.txt", restrictedToMinimumLevel: LogEventLevel.Information)
                //.WriteTo.TextWriter(_messages)
            }
            else
            {
                log.WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information);
            }

            Serilog.Log.Logger = log.CreateLogger();

            Core.InitPlugins();

            foreach (string file in FileList)
            {
                Log.Information("Extracting: " + file);
                Core.ExtractFile(file);
            }
        }

        public class CommandLineOptions
        {
            //BUG: not catching required
            [Value(0, MetaName = "input file", HelpText = "Input file to be processed.", Required = true)]
            public IEnumerable<string> FileName { get; set; }

            //TODO: change to string to match serilog?
            [Option(shortName: 'v', longName: "verbose", HelpText = "Print out all messages", Required = false, Default = false)]
            public bool Verbose { get; set; }

            [Option(shortName: 'o', longName: "overwrite", HelpText = "Overwrite all files if exists", Required = false, Default = false)]
            public bool OverwriteAll { get; set; }

            [Option(shortName: 'r', longName: "rename", HelpText = "Auto rename existing files", Required = false, Default = false)]
            public bool AutoRename { get; set; }

            [Option(shortName: 'c', longName: "raw", HelpText = "Do not decompile scripts", Required = false, Default = false)]
            public bool Raw { get; set; }

            //TODO: add output dir?, optional log file
        }

        private static int ParseSuccess(CommandLineOptions options)
        {
            foreach (string path in options.FileName)
            {
                FileList.Add(path);
            }
            //Pop first argument (assembly name) from file list
            FileList.RemoveAt(0);

            Verbose = options.Verbose;
            //OverwriteAll = options.OverwriteAll;
            //AutoRename = options.AutoRename;
            //Raw = options.Raw;
            Helpers.setAutoRename(options.AutoRename);
            Helpers.setOverwriteAll(options.OverwriteAll);
            Helpers.setRaw(options.Raw);
            return 1;
        }

        private static int ParseFail(IEnumerable<CommandLine.Error> errs)
        {
            foreach (CommandLine.Error err in errs)
                Serilog.Log.Fatal($"CLI error of: '{err}");
            return 1;
        }
    }
}