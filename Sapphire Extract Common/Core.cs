using CommandLine;
using System;
using System.Collections.Generic;

namespace Sapphire_Extract_Common
{
    /// <summary>
    /// Common functions between the CLI and GUI
    /// </summary>
    public class Core
    {
        //parse cmd line. go strait to extraction on gui? or set overwrite/etc?
        //wait till main window post init and trigger populate with cmd
        //make cli class called Opts ie Opts.overwrite

        //have variable input files. core should take in list or single file name and marshal
        //  the (later) threads and trigger the extraction events (also in core)

        //fileList technically private, as is an array, and plugin operates on single file in parameter
        public static List<string> FileList { get; set; } = new List<string>();
        public static bool Verbose { get; set; }
        public static bool OverwriteAll { get; set; }
        public static bool AutoRename { get; set; }
        public static bool Raw { get; set; }

        public class CommandLineOptions
        {
            [Value(0, MetaName = "input file", HelpText = "Input file to be processed.", Required = false)]
            public IEnumerable<string>FileName { get; set; }

            //TODO: change to string to match serilog?
            [Option(shortName: 'v', longName: "verbose", HelpText = "Print out all messages", Required = false, Default = false)]
            public bool Verbose { get; set; }

            [Option(shortName: 'o',  longName: "overwrite", HelpText = "Overwrite all files if exists", Required = false, Default = false)]
            public bool OverwriteAll { get; set; }

            [Option(shortName: 'r', longName: "rename", HelpText = "Auto rename existing files", Required = false, Default = false)]
            public bool AutoRename { get; set; }

            [Option(shortName: 'c', longName: "raw", HelpText = "Do not decompile scripts", Required = false, Default = false)]
            public bool Raw { get; set; }

            //TODO: add output dir?
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
            OverwriteAll = options.OverwriteAll;
            AutoRename = options.AutoRename;
            Raw = options.Raw;
            return 1;
        }

        /// <summary>
        /// Parse CLI arguments and init plugins.
        /// </summary>
        public static void init()
        {
            Parser.Default.ParseArguments<CommandLineOptions>(Environment.GetCommandLineArgs()).MapResult(
                options => ParseSuccess(options), _ => 1);
        }
    }
}