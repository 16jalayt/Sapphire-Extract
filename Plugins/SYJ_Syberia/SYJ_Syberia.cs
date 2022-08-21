using Plugin_Contract;
using Sapphire_Extract_Helpers;
using System.IO;
using System.Linq;

//Version 1.0

/*Remember to build solution manually before testing plugin!
 * Start debugging does NOT rebuild plugins!
 * 
 * This is an exmple plugin to show the basics of creating a plugin.
 *The example file is included in the example plugin directory.
 **/
namespace SYJ_Syberia
{
    internal class SYJ_Syberia : IPlugin
    {
        /// <summary>
        /// Pretty text that shows in the error logs to identify the plugin.
        /// </summary>
        public string Name { get { return "SYJ Syberia"; } }

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
            //If the file has wrong extension, say we can't extract.
            if (Path.GetExtension(InStream.FileName) == ".syj")
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
            //This just appends the jpg header onto the file. 
            //It works because all syj files are backgrounds of
            //the same resolution?


            //Thanks to:
            // Syberia I
            // Alex kalumb1@ya.ru
            // SYJ to JPG converter by -=CHE@TER=-
            // http://www.ctpax-x.org/

            //Log.Warning($"Plugin '{Name}' is not finished. Will likely spew out garbage.");



            /*byte[] jpg_header = new byte[] { 0xff, 0xd8, 0xff, 0xe0, 0x00, 0x10, 0x4a, 0x46, 0x49, 0x46 };
            byte[] FileContents = InStream.ReadBytes((int)InStream.Length());
            byte[] output = new byte[(int)InStream.Length() + jpg_header.Length];
            Array.Copy(jpg_header, 0, output, 0, jpg_header.Length);
            Array.Copy(FileContents, 0, output, jpg_header.Length, output.Length);*/

            byte[] jpg_header = new byte[] { 0xff, 0xd8, 0xff, 0xe0, 0x00, 0x10, 0x4a, 0x46, 0x49, 0x46 };
            byte[] FileContents = InStream.ReadBytes((int)InStream.Length());
            byte[] output = jpg_header.ToList().Concat(FileContents.ToList()).ToArray();

            Helpers.Write(InStream.FilePath, InStream.FileNameWithoutExtension + ".jpg", output, false);

            return true;
        }
    }
}