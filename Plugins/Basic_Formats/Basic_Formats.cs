using Plugin_Contract;
using Sapphire_Extract_Helpers;
using Serilog;
using System.IO;
using System.IO.Compression;

//Version 1.0

/*Remember to build solution manually before testing plugin!
 * Start debugging does NOT rebuild plugins!
 *
 * This is an exmple plugin to show the basics of creating a plugin.
 *The example file is included in the example plugin directory.
 **/

namespace Basic_Formats
{
    internal class Basic_Formats : IPlugin
    {
        /// <summary>
        /// Pretty text that shows in the error logs to identify the plugin.
        /// </summary>
        public string Name
        { get { return "Basic_Formats"; } }

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
            //List known file types Zips:
            //base.scs - SCS software games - Bus Driver, Euro Truck Sim
            //assets.wwx common.wwx - Sandlot Games - Westward 2-4 + Kingdoms
            if (Helpers.AssertString(InStream, "PK"))
                return true;

            InStream.Seek(0);
            //List known file types Bitmaps:
            //*.dat - Midnight Synergy - Wonderland
            if (Helpers.AssertString(InStream, "BM"))
                return true;

            InStream.Seek(0);
            //List known file types Wavs:
            //*.M, *.S - MECC - Oregon Trail Windows
            //.DAT - Roller Coaster Tycoon
            if (Helpers.AssertString(InStream, "RIFF"))
            {
                InStream.Seek(8);
                if (Helpers.AssertString(InStream, "WAVEfmt"))
                    return true;
                return false;
            }
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
            if (Helpers.AssertString(InStream, "PK"))
            {
                Log.Information($"Currently extracting the file: '{InStream.FileName}' as a zip. Please be patient.");

                //Have to close inStream to pass to another stream
                InStream.Dispose();

                ZipFile.ExtractToDirectory(InStream.FilePath, InStream.FileDirectory + "\\" + InStream.FileNameWithoutExtension, true);
                return true;
            }
            InStream.Seek(0);

            //Both are just an easy rename
            if (Helpers.AssertString(InStream, "BM"))
            {
                SaveFile(InStream, ".bmp");
                return true;
            }
            InStream.Seek(0);

            if (Helpers.AssertString(InStream, "RIFF"))
            {
                SaveFile(InStream, ".wav");
                return true;
            }

            return false;
        }

        private void SaveFile(BetterBinaryReader InStream, string extension)
        {
            InStream.Seek(0);
            byte[] FileContents = InStream.ReadBytes((int)InStream.Length());
            Helpers.Write(InStream.FilePath, InStream.FileNameWithoutExtension + extension, FileContents, false);
        }
    }
}