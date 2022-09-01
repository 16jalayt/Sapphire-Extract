using Plugin_Contract;
using Sapphire_Extract_Helpers;
using Serilog;

//Version 1.0

/*Remember to build solution manually before testing plugin!
 * Start debugging does NOT rebuild plugins!
 * 
 * This is an exmple plugin to show the basics of creating a plugin.
 *The example file is included in the example plugin directory.
 **/
namespace DATA_Sandlot
{
    internal class DATA_Sandlot : IPlugin
    {
        /// <summary>
        /// Pretty text that shows in the error logs to identify the plugin.
        /// </summary>
        public string Name { get { return "Sandlot DATA and ZDATA"; } }

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
            //If the file has wrong id, say we can't extract
            //Note: can also get the file extension in case no magic
            //It is prefered to identify by content not name for reliability
            //if (Path.GetExtension(InStream.FileName) == ".demo")
            if (Helpers.AssertString(InStream, "DEMO"))
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
            Log.Warning($"Plugin '{Name}' is not finished. Will likely spew out garbage.");
           
            return true;
        }
    }
}