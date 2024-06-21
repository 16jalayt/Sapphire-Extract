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

namespace DEMO_ExamplePlugin
{
    internal class DEMO_ExamplePlugin : IPlugin
    {
        /// <summary>
        /// Pretty text that shows in the error logs to identify the plugin.
        /// </summary>
        public string Name
        { get { return "Example Plugin"; } }

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
            //If the file has wrong id, say we can't extract
            //Note: can also get the file extension in case no magic
            //It is prefered to identify by content and not name for reliability
            //if (Path.GetExtension(InStream.FileName) == ".demo")
            //if (Helpers.AssertValue(InStream, new byte[] { 0xAC, 0xDC }))
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
            Log.Warning($"Put important info here like validated version numbers or warnings to users.");

            //Seek past magic (DEMO)
            InStream.Seek(4);

            //Note: best way to check for constant bytes is AssertValue,
            //but can also use AssertString, like under CanExtract above,
            //but AssertString only works with plain text

            //Note: AssertValue is designed as a notification to the developer of
            //different file variations to aid in research. A FAILED ASSERT WILL NOT END PLUGIN! just display a warning.
            //However, it will return a bool, so you can use an "if(!assert) return false" to fail the extraction.

            //Version 2.1
            Helpers.AssertValue(InStream, new byte[] { 0x02, 0x01 });

            //# of files (4 bytes)
            int NumFiles = InStream.ReadInt();

            Log.Information($"Extracting {NumFiles} files...\n");

            //End of entry in file table
            //Note:need forward dec so persists each loop.
            long TableOffset;

            for (int i = 0; i < NumFiles; i++)
            {
                //Offset of current file in container
                //The read functions will also take an optional string
                //that will be logged at debug level. This method is prefered.
                //The alternative is the next ReadInt().
                int FileOffset = InStream.ReadInt("File offset: ");

                int FileLength = InStream.ReadInt();
                Log.Debug($"File Length: {FileLength}");

                //Name of current output file, and remove \0
                //Note: In the demo, the name is fixed at 15 chars and null padded.
                //Note: \n on last print line for file to space output print.
                string CurrFileName = Helpers.String(InStream.ReadBytes(15)).Trim('\0');
                Log.Debug($"CurrFileName: {CurrFileName}\n");

                TableOffset = InStream.Position();
                //Go to start of file
                InStream.Seek(FileOffset);

                byte[] FileContents = InStream.ReadBytes(FileLength);
                Helpers.Write(InStream.FilePath, CurrFileName, FileContents);

                //Go back to look up table
                InStream.Seek(TableOffset);
            }

            //Return that extraction was sucessful.
            //If there is a faliure condition, you can return false,
            //and the core will try a different plugin.
            return true;
        }
    }
}