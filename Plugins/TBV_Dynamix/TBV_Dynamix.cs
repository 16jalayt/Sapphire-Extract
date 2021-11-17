using Plugin_Contract;
using Sapphire_Extract_Helpers;
using Serilog;

namespace TBV_Dynamix
{
    //Trophy Bass Volume
    internal class TBV_Dynamix : IPlugin
    {
        /// <summary>
        /// Pretty text that shows in the error logs to identify the plugin.
        /// </summary>
        public string Name
        { get { return "TBV Dynamix"; } }

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
            if (Helpers.AssertString(InStream, "TBVolume"))
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
            //Log.Warning($"Plugin '{Name}' is not finished. Will likely spew out garbage.");
            Log.Warning($"Some files may error with an EOF exception.This appears to be Dynamix's fault.");

            //Seek past magic (TBVolume)
            InStream.Seek(9);

            //Unknown value. Seems to always stay same. Version?
            Helpers.AssertValue(InStream, new byte[] { 0xD0, 0x07 });

            //# of files (4 bytes)
            int NumFiles = InStream.ReadInt();

            Log.Information($"Extracting {NumFiles} files...\n");
            //Always null?
            Helpers.AssertValue(InStream, new byte[] { 0x00, 0x00 });

            //Dev's name/email null padded? RichRayl@CUC\0\0...
            Helpers.AssertValue(InStream, new byte[] { 0x52, 0x69, 0x63, 0x68, 0x52, 0x61, 0x79, 0x6C, 0x40, 0x43, 0x55, 0x43, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

            //End of entry in file table
            long TableOffset;

            for (int i = 0; i < NumFiles; i++)
            {
                //Unknown. Different for each file
                InStream.Skip(4);

                //Offset of current file in container
                int FileOffset = InStream.ReadInt();
                Log.Debug($"File offset: {FileOffset}");
                TableOffset = InStream.Position();

                //Go to start of file
                InStream.Seek(FileOffset);
                //Name of current output file, and remove \0
                string CurrFileName = Helpers.String(InStream.ReadBytes(24)).Trim('\0');
                //string CurrFileName = Helpers.String(InStream.ReadBytes(24)).Replace("\0", string.Empty);
                Log.Debug($"CurrFileName: {CurrFileName}");

                int FileLength = InStream.ReadInt();
                Log.Debug($"File Length: {FileLength}\n");

                byte[] FileContents = InStream.ReadBytes(FileLength);
                Helpers.Write(InStream.FilePath, CurrFileName, FileContents);

                //Go back to look up table
                InStream.Seek(TableOffset);
            }

            return true;
        }

        public int GetPriority()
        {
            return 100;
        }
    }
}