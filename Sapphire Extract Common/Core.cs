using CommandLine;
using McMaster.NETCore.Plugins;
using Plugin_Contract;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

    private static List<IPlugin> PluginList= new List<IPlugin>();

        public class CommandLineOptions
        {
            //BUG: not catching required
            [Value(0, MetaName = "input file", HelpText = "Input file to be processed.", Required = true)]
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
            OverwriteAll = options.OverwriteAll;
            AutoRename = options.AutoRename;
            Raw = options.Raw;
            return 1;
        }

        private static int ParseFail(IEnumerable<CommandLine.Error> errs)
        {
            foreach(CommandLine.Error err in errs)
                Serilog.Log.Fatal($"CLI error of: '{err}");
            return 1;
        }

        /// <summary>
        /// Parse CLI arguments and init plugins.
        /// </summary>
        public static void init()
        {
            //Parse cli
            Parser.Default.ParseArguments<CommandLineOptions>(Environment.GetCommandLineArgs()).MapResult(
                options => ParseSuccess(options), err => ParseFail(err));

            //Load plugins after known valid usage
            var loaders = new List<PluginLoader>();

            // create plugin loaders
            var pluginsDir = Path.Combine(AppContext.BaseDirectory, "plugins");
            foreach (var dir in Directory.GetDirectories(pluginsDir))
            {
                var dirName = Path.GetFileName(dir);
                var pluginDll = Path.Combine(dir, dirName + ".dll");
                if (File.Exists(pluginDll))
                {
                    var loader = PluginLoader.CreateFromAssemblyFile(
                        pluginDll,
                        sharedTypes: new[] { typeof(IPlugin) });
                    loaders.Add(loader);
                }
            }

            // Create an instance of plugin types
            foreach (var loader in loaders)
            {
                foreach (var pluginType in loader
                    .LoadDefaultAssembly()
                    .GetTypes()
                    .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsAbstract))
                {
                    // This assumes the implementation of IPlugin has a parameterless constructor
                    var plugin = Activator.CreateInstance(pluginType) as IPlugin;

                    plugin.init(Log.ForContext("SourceContext", "myDll"));
                    Log.Information($"Loaded plugin: '{plugin?.Name}'.");
                    PluginList.Add(plugin);
                }
            }
        }

        public static void ExtractFile(string FileName)
        {
            //TODO: err handling on input
            using (FileStream fs = new FileStream(FileName, FileMode.Open))
            using (BinaryReader InStream = new BinaryReader(fs, Encoding.Default))
            {
                foreach (var plugin in PluginList)
                {
                    if (plugin.CanExtract(InStream))
                    {
                        Log.Information($"Attempting to extract  '{plugin?.Name}'.");
                        //Attempt to extract
                        if (plugin.Extract(InStream))
                            return;
                        //Failed extraction
                        else
                        {
                            Log.Error($"Failed to extract: '{FileName}' using plugin: '{plugin?.Name}'. Trying next plugin.");
                        }

                    }
                }
                //Exit loop if failed to return during sucess.
                //This means no available plugins...
                Log.Fatal($"Failed to extract: '{FileName}'. Not sucessful with any plugins.");
            }
        }

        public static void DetectExtension(BinaryReader InStream)
        {
            InStream.BaseStream.Seek(0,SeekOrigin.Begin);

        }
    }
}