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

        public class CommandLineOptions
        {
            [Value(0, MetaName = "input file", HelpText = "Input file to be processed.", Required = false)]
            public IEnumerable<string>FileName { get; set; }

            //TODO: change to string to match serilog?
            [Option(shortName: 'v', longName: "verbose", Required = false, HelpText = "Print out all messages", Default = false)]
            public bool Verbose { get; set; }

            //@Parameter(names = { "-a", "-o", "--overwrite" }, description = "Overwrite all files")
            //public boolean overwriteAll = false;

            // @Parameter(names = { "-r", "--rename"}, description = "Auto rename existing files")
            //public boolean autoRename = false;

            //@Parameter(names = { "-v", "--verbose" }, description = "Print out extra information")
            //public boolean verbose = false;

            //@Parameter(names = { "-c", "--raw" }, description = "Skip decompalation. Dump compiled script")
            //public boolean raw = false;

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