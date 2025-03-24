using Plugin_Contract;
using Sapphire_Extract_Helpers;
using Serilog;
using System.IO;

namespace Frogwares
{
    //Taken from "Journey to the Center of the Earth".
    //Not sure if exists in other games
    internal class Frogwares : IPlugin
    {
        private struct TableEntry
        {
            public int off;
            public int len;
            public string name;
        }

        /// <summary>
        /// Pretty text that shows in the error logs to identify the plugin.
        /// </summary>
        public string Name
        { get { return "0000 Frogwares"; } }

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
            if (Path.GetExtension(InStream.FileName) == ".0000")
                if (Helpers.AssertString(InStream, "package"))
                    return true;

            return false;
        }

        /// <summary>
        /// Extracts the file from the given stream.
        /// </summary>
        /// <param name="InStream"></param>
        /// <returns></returns>
        public bool Extract(BetterBinaryReader InStream)
        {
            //Skip magic
            InStream.Seek(7);
            //version?
            Helpers.AssertInt(InStream, 1, "Version seems to be different than 1");
            //Unknown
            InStream.Skip(8);
            int numEntries = InStream.ReadInt("Entries: ");
            //Unknown
            InStream.Skip(5);

            TableEntry[] entries = new TableEntry[numEntries];

            for (int i = 0; i < numEntries; i++)
            {
                Log.Debug("\n");
                entries[i].off = InStream.ReadInt("Offset: ");
                entries[i].len = InStream.ReadInt("Length: ");
                int namelen = InStream.ReadInt();
                entries[i].name = Helpers.String(InStream.ReadBytes(namelen));
                Log.Debug($"CurrFileName: {entries[i].name}");
            }

            long EndOfTable = InStream.Position();

            for (int i = 0; i < numEntries; i++)
            {
                //Go back to look up table
                InStream.Seek(entries[i].off + EndOfTable);

                //Unknown
                InStream.Skip(13);

                byte[] FileContents = InStream.ReadBytes(entries[i].len);
                Helpers.Write(InStream.FilePath, entries[i].name, FileContents);
            }

            return true;
        }
    }
}