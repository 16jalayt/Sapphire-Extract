using Plugin_Contract;
using Sapphire_Extract_Helpers;
using Serilog;
using System.IO;
using System.IO.Compression;

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
            if (Path.GetExtension(InStream.FileName) == ".xml")
                return true;
            if (Helpers.AssertValue(InStream, new byte[] { 0xAC, 0xDC }))
                return true;
            InStream.Seek(0);
            if (Helpers.AssertValue(InStream, new byte[] { 0x1F, 0x8B }))
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
            //if (Path.GetExtension(InStream.FileName) == ".xml")
            //return xmltest(InStream);
            if (Helpers.AssertValue(InStream, new byte[] { 0xAC, 0xDC }))
                return ExtractData(InStream);
            else
                return ExtractZData(InStream);
        }

        //TODO: xmls are still giberish. XOR? Is not Gzip.
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

        //An attempt to test if XMLs were XORd
        /*public bool xmltest(BetterBinaryReader InStream)
        {
            InStream.Seek(0);
            byte[] FileContents = InStream.ReadBytes((int)InStream.Length());
            byte[] FileContentsOutput = new byte[FileContents.Length];
            for (int i = 0; i < FileContents.Length; i++)
            {
                byte original = FileContents[i];
                //Log.Information($"Original {Convert.ToString(original, 2)}\n");
                //byte shift1 = FileContents[0];
                //Log.Information($"shift1 {Convert.ToString(shift1, 2)}\n");

                //uint shift2 = BitOperations.RotateRight(original, 2);
                //byte shift2 = Helpers.BitRotateRight(original,2);
                byte shift2 = (byte)Helpers.BitRotateRight(original, 2);
                //Log.Information($"shift2 {Convert.ToString(shift2, 2)}\n");

                byte xor = (byte)((byte)shift2 ^ (0b11010101) + i);
                //Log.Information($"xor {Convert.ToString(xor, 2)}\n");
                FileContentsOutput[i] = xor;
            }
            Helpers.Write(InStream.FilePath, InStream.FileName + ".txt", FileContentsOutput, false);

            return true;
        }*/

        public bool ExtractZData(BetterBinaryReader InStream)
        {
            InStream.Seek(0);
            //Pure gzip stream, then pass to ExtractData()
            using (GZipStream decompressionStream = new GZipStream(InStream.GetStream(), CompressionMode.Decompress))
            {
                BetterBinaryReader uncompressedFile = new BetterBinaryReader(decompressionStream, InStream);
                ExtractData(uncompressedFile);
            }

            return true;
        }
    }
}