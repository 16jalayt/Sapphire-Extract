using McMaster.NETCore.Plugins;
using Plugin_Contract;
using Sapphire_Extract_Helpers;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        private static List<IPlugin> PluginList = new List<IPlugin>();
        //private static SimplePriorityQueue<IPlugin> PluginQueue = new SimplePriorityQueue<IPlugin>();

        /// <summary>
        /// Parse CLI arguments and init plugins.
        /// </summary>
        public static void InitPlugins()
        {
            //Load plugins after known valid usage
            var loaders = new List<PluginLoader>();

            //TODO: move out of subfolders?
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

                    plugin.Init(Log.ForContext("SourceContext", "myDll"));
                    //TODO: replace with dll name?
                    Log.Verbose($"Loaded plugin: '{plugin?.Name}'.");
                    PluginList.Add(plugin);
                    //PluginQueue.Enqueue(plugin, plugin.Priority);
                }
            }
        }

        /// <summary>
        /// Attempt extraction of a given file.
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns>sucess</returns>
        public static bool ExtractFile(string FileName)
        {
            if (!File.Exists(FileName))
            {
                Log.Error($"The file: '{FileName}' does not exist.\n");
                return false;
            }

            BetterBinaryReader InStream = new BetterBinaryReader(FileName);

            //while(PluginQueue.Count != 0)
            foreach (IPlugin plugin in PluginList)
            {
                //IPlugin plugin = PluginQueue.Dequeue();

                InStream.Seek(0);
                if (plugin.CanExtract(InStream))
                {
                    Log.Information($"Attempting to extract file: '{FileName}' with plugin: '{plugin?.Name}'.\n");
                    InStream.Seek(0);
                    //Attempt to extract
                    if (plugin.Extract(InStream))
                    {
                        InStream.Dispose();
                        return true;
                    }

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
            InStream.Dispose();
            return false;
        }
    }
}