using Plugin_Contract;
using Sapphire_Extract_Helpers;
using Serilog;

namespace PAK_Minigolf
{
    internal class PAK_Minigolf : IPlugin
    {
        /// <summary>
        /// Pretty text that shows in the error logs to identify the plugin.
        /// </summary>
        public string Name
        { get { return "PAK Minigolf"; } }

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
            if (Helpers.AssertString(InStream, "tongas_pack_v20000"))
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

            //Seek past magic (tongas_pack_v20000)
            Helpers.AssertString(InStream, "tongas_pack_v20000");

            //# of files (4 bytes)
            int NumFiles = InStream.ReadInt();
            Log.Information($"Extracting {NumFiles} files...\n");

            //Unknown value. Need bigger sample size than 1
            Helpers.AssertInt(InStream, 199208);

            //End of entry in file table
            long TableOffset;

            for (int i = 0; i < NumFiles; i++)
            {
                //Offset of current file in container
                int FileOffset = InStream.ReadInt("File offset: ");
                int FileLength = InStream.ReadInt("File Length: ");
                int PathLength = InStream.ReadInt("Path Length: ");

                string CurrFileName = Helpers.String(InStream.ReadBytes(PathLength));
                Log.Information($"CurrFileName: {CurrFileName}");

                TableOffset = InStream.Position();

                //Go to start of file
                InStream.Seek(FileOffset);

                byte[] FileContents = InStream.ReadBytes(FileLength);
                //LZMA compressed
                Helpers.Write(InStream.FilePath, CurrFileName, Compression_Manager.LZ4DecompressChunk(FileContents));

                Log.Debug("");

                //Go back to look up table
                InStream.Seek(TableOffset);
            }

            //Return that extraction was sucessful.
            return true;
        }
    }
}