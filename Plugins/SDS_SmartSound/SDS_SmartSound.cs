using Plugin_Contract;
using Sapphire_Extract_Helpers;
using Serilog;

//Version 1.0

namespace SDS_SmartSound
{
    internal class SDS_SmartSound : IPlugin
    {
        /// <summary>
        /// Pretty text that shows in the error logs to identify the plugin.
        /// </summary>
        public string Name
        { get { return "SDS Smart Sound"; } }

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
            if (Helpers.AssertString(InStream, "SDSS"))
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
            //We just need to change the magic
            byte[] FileContents = InStream.ReadBytes((int)InStream.Length());
            FileContents[0] = (byte)'R';
            FileContents[1] = (byte)'I';
            FileContents[2] = (byte)'F';
            FileContents[3] = (byte)'F';
            Helpers.Write(InStream.FilePath, InStream.FileNameWithoutExtension + ".wav", FileContents, false);

            return true;
        }
    }
}