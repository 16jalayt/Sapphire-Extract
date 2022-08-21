using Plugin_Contract;
using Sapphire_Extract_Helpers;
using Serilog;
using System.IO.Compression;

//Version 1.0

/*Remember to build solution manually before testing plugin!
 * Start debugging does NOT rebuild plugins!
 * 
 * This is an exmple plugin to show the basics of creating a plugin.
 *The example file is included in the example plugin directory.
 **/
namespace ZIP_Genaric
{
    internal class ZIP_Genaric : IPlugin
    {
        /// <summary>
        /// Pretty text that shows in the error logs to identify the plugin.
        /// </summary>
        public string Name { get { return "Example Plugin"; } }

        //Unimplemented
        /*/// <summary>
        /// Get the priority of the plugin. Lower is higher priority. Normal Priority: 100
        /// </summary>
        public int Priority { get { return 100; } }*/

        /// <summary>
        /// Called when plugins are enumerated.
        /// </summary>
        /// <param name="masterlogger"></param>
        /// <returns></returns>
        public bool Init(Serilog.ILogger masterlogger)
        {
            Serilog.Log.Logger = new Serilog.LoggerConfiguration()
          .MinimumLevel.Debug()
          .WriteTo.Logger(masterlogger)
          .CreateLogger();

            return true;
        }

        /// <summary>
        /// Tests whether this plugin will accept the current file.
        /// </summary>
        /// <param name="InStream"></param>
        /// <returns></returns>
        public bool CanExtract(BetterBinaryReader InStream)
        {
            //List known file types here:
            //base.scs - SCS software games - Bus Driver, Euro Truck Sim
            //assets.wwx common.wwx- Sandlot Games - Westward 2-4 + Kingdoms
            if (Helpers.AssertString(InStream, "PK"))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Extracts the file from the given stream.
        /// </summary>
        /// <param name="InStream"></param>
        /// <returns></returns>
        public bool Extract(BetterBinaryReader InStream)
        {
            Log.Information($"Currently extracting the file: '{InStream.FileName}' as a zip. Please be patient.");

            //Have to close inStream to pass to another stream
            InStream.Dispose();

            ZipFile.ExtractToDirectory(InStream.FilePath, InStream.FileDirectory + "\\" + InStream.FileNameWithoutExtension, true);

            return true;
        }
    }
}