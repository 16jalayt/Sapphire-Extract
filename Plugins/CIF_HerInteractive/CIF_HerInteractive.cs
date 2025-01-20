using Plugin_Contract;
using Sapphire_Extract_Helpers;
using Serilog;

//Version 1.0

namespace CIF_HerInteractive
{
    internal class CIF_HerInteractive : IPlugin
    {
        /// <summary>
        /// Pretty text that shows in the error logs to identify the plugin.
        /// </summary>
        public string Name
        { get { return "CIF Her Interactive"; } }

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
            if (Helpers.AssertString(InStream, "Nancy or whatever"))
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

            //Seek past magic (DEMO)
            InStream.Seek(4);

            //Unknown value. Seems to always stay same. Version likely 2.1
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
                int FileOffset = InStream.ReadInt();
                Log.Debug($"File offset: {FileOffset}");

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