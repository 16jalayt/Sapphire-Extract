using Plugin_Contract;
using Sapphire_Extract_Helpers;
using Serilog;
using System.IO;
using System.Text;

//Version 1.0
namespace DATA_Sandlot
{
    internal class DATA_Sandlot : IPlugin
    {
        /// <summary>
        /// Pretty text that shows in the error logs to identify the plugin.
        /// </summary>
        public string Name
        { get { return "Sandlot DATA and ZDATA"; } }

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
            if (Helpers.AssertValue(InStream, new byte[] { 0xAC, 0xDC }) || Helpers.AssertValue(InStream, new byte[] { 0x1F, 0x8B }))
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

            if (Helpers.AssertValue(InStream, new byte[] { 0xAC, 0xDC }))
                return ExtractData(InStream);
            else
                return ExtractZData(InStream);
        }

        //TODO: xmls are still giberish. XOR?
        public bool ExtractData(BetterBinaryReader InStream)
        {
            InStream.Seek(4);
            int numfiles = InStream.ReadInt("# files:");

            long tableOffset = InStream.Position();

            for (int i = 0; i < numfiles; i++)
            {
                InStream.Seek(tableOffset);

                //Filename is a null termed string. hard to do in language other than c
                string CurrFileName = InStream.ReadNullTerminatedString();
                Log.Information($"Extracting {CurrFileName}\n");

                int fileOffset = InStream.ReadInt("File offset:");
                int fileLen = InStream.ReadInt("File Len:");

                tableOffset = InStream.Position();
                //Go to start of file
                InStream.Seek(fileOffset);

                byte[] FileContents = InStream.ReadBytes(fileLen);

                Helpers.Write(InStream.FilePath, CurrFileName, FileContents);
            }
            return true;
        }

        public bool ExtractZData(BetterBinaryReader InStream)
        {
            InStream.Seek(0);
            //TODO: Pure gzip stream, then pass to ExtractData()
            return true;
        }
    }
}