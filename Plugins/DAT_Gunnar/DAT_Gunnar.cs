using Plugin_Contract;
using Sapphire_Extract_Helpers;
using Serilog;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace DAT_Gunnar
{
    //IMPORTANT: no magic or identification. Make extra sure to validate data.
    internal class DAT_Gunnar : IPlugin
    {
        /// <summary>
        /// Pretty text that shows in the error logs to identify the plugin.
        /// </summary>
        public string Name { get { return "DAT Gunnar"; } }

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
            //If the file has wrong id, say we can't extract.
            if (Path.GetExtension(InStream.FileName) == ".dat")
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
            //REIMPORTANT: no magic or identification. Make extra sure to validate data.
            //Log.Warning($"Plugin '{Name}' is not finished. Will likely spew out garbage.");
            Log.Warning($"DAT_Gnnar files have no means of identification. This may not be the correct archive type.");

            //# of files (4 bytes)
            int NumFiles = InStream.ReadInt();
            Log.Information($"Extracting {NumFiles} files...\n");
            if (!(NumFiles < 500) && !(NumFiles > 1))
            {
                Log.Error($"{NumFiles} files is not a reasonable amount\n This is probably not a DAT_Gunnar archive. Failing.\n");
                return false;
            }

            //Skip null padding
            InStream.Seek(32);

            long TableOffset = InStream.Position();

            //Test if C wide string by looking if second byte of 1st letter is null
            InStream.Skip(1);
            bool IsWide = false;
            if (InStream.ReadBytes(1)[0]==0)
                IsWide = true;
            //Go back to table entry start.
            InStream.Seek(TableOffset);

            for (int i = 0; i < NumFiles; i++)
            {
                string CurrFileName;
                //Name of current output file, and remove 0xCD padding. String converted to utf-8 already so is unknown char.
                if (IsWide)
                    CurrFileName = Helpers.String(InStream.ReadBytes(512)).Trim('�').Replace("\0", string.Empty);
                else
                    CurrFileName = Helpers.String(InStream.ReadBytes(256)).Trim('�').Replace("\0", string.Empty);
                //string CurrFileName = Helpers.String(InStream.ReadBytes(256)).Replace("\xCD", string.Empty);

                CurrFileName = CurrFileName.Substring(CurrFileName.IndexOf('\\') + 1);
                Log.Debug($"CurrFileName: {CurrFileName}");

                //Offset of current file in container
                int FileOffset = InStream.ReadInt();
                Log.Debug($"File offset: {FileOffset}");
                int FileLength = InStream.ReadInt();
                Log.Debug($"File length: {FileLength}");

                //Unknown int and 20 padding
                if (!IsWide)
                    InStream.Skip(24);

                TableOffset = InStream.Position();

                //Go to start of file
                InStream.Seek(FileOffset);

                byte[] FileContents = InStream.ReadBytes(FileLength);
                Helpers.Write(InStream.FilePath, CurrFileName, FileContents);

                //Go back to look up table
                InStream.Seek(TableOffset);
            }

            return true;
        }
    }
}