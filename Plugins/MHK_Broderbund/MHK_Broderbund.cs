using Plugin_Contract;
using Sapphire_Extract_Helpers;
using Serilog;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MHK_Broderbund
{
    internal class MHK_Broderbund : IPlugin
    {
        //Great resource: https://web.archive.org/web/20221004204905/https://insidethelink.ortiche.net/wiki/index.php/Mohawk_archive_format
        /// <summary>
        /// Pretty text that shows in the error logs to identify the plugin.
        /// </summary>
        public string Name
        { get { return "MHK Broderbund"; } }

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
            //TODO: shouldn't create logger again. Should be managed by core
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
            //MHWK
            if (Helpers.AssertValue(InStream, [0x4D, 0x48, 0x57, 0x4B]))
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
            //IMPORTANT: Everything is BIG endian
            Log.Warning($"Plugin '{Name}' is not finished. Will likely spew out garbage.\n");

            InStream.Seek(0);

            //MHWK
            if (!Helpers.AssertValue(InStream, [0x4D, 0x48, 0x57, 0x4B]))
            {
                Log.Error("Invalid magic. Expected MHWK");
                return false;
            }

            //Length from after this int to end of file
            int archiveLen = InStream.ReadIntBE();
            if (archiveLen != InStream.Length() - 8)
            {
                Log.Error("Invalid archive length sans header. Please report this.");
            }

            //RSRC
            if (!Helpers.AssertValue(InStream, [0x52, 0x53, 0x52, 0x43]))
            {
                Log.Error("Invalid magic. Expected RSRC");
                return false;
            }

            //Version
            if (!Helpers.AssertShortBE(InStream, 256))
            {
                Log.Error("Unimplemented version. Please report this.");
                return false;
            }

            //Not sure what this is for. insidethelink says not used for reading
            if (!Helpers.AssertShortBE(InStream, 0))
            {
                Log.Error("Unimplemented compaction. Please report this.");
            }

            //Total archive length
            int archiveLenAgain = InStream.ReadIntBE();
            if (archiveLenAgain != InStream.Length())
            {
                Log.Error("Invalid archive length. Please report this.");
            }

            //Resource dir offset
            int resourceDirOff = InStream.ReadIntBE();

            //Location of file table in resource dir
            short fileTableOff = InStream.ReadShortBE();

            //Len of file table
            short fileTableLen = InStream.ReadShortBE();

            InStream.Seek(resourceDirOff);

            //Offset of the name table in the resource dir
            short nameTableOff = InStream.ReadShortBE();

            //Num file types in archive
            //Each type of file has it's own directories
            //Different games have very different file types
            short numResourceTypes = InStream.ReadShortBE();

            for (int i = 0; i < numResourceTypes; i++)
            {
                string resourceType = Helpers.String(InStream.ReadBytes(4));
                short resourceTypeDirOff = InStream.ReadShortBE();
                short resourceTypeNameDirOff = InStream.ReadShortBE();

                InStream.Seek(resourceDirOff + resourceTypeDirOff);

                short numResEntries = InStream.ReadShortBE();
                //TODO: combine ID and name as tuple?
                List<int> resIDs = new List<int>();

                for (int r = 0; r < numResEntries; r++)
                {
                    short resID = InStream.ReadShortBE();
                    //Starting at 1
                    if (!Helpers.AssertShortBE(InStream, (short)(r + 1)))
                    {
                        Log.Error("Res table index does not match loop");
                    }

                    resIDs.Add(resID);
                }

                InStream.Seek(resourceTypeNameDirOff + resourceDirOff);
                //TODO: Should be 0
                short numNameEntries = InStream.ReadShortBE();
                List<int> resNames = new List<int>();

                if (numNameEntries != 0)
                    Log.Error("File has a name table");

                for (int r = 0; r < numNameEntries; r++)
                {
                    short nameOff = InStream.ReadShortBE();
                    short resID = InStream.ReadShortBE();

                    resNames.Add(resID);
                    //TODO:
                }

                InStream.Seek(fileTableOff + resourceDirOff);
                int numFileEntries = InStream.ReadIntBE();

                for (int r = 0; r < numFileEntries; r++)
                {
                    int fileOff = InStream.ReadIntBE();
                    short fileLen = InStream.ReadShortBE();

                    //short resource data size, bits 15 - 0
                    //byte resource data size, bits 23 - 16
                    //byte resource flags(unknown)
                    //unsigned short unknown(usually zero in Riven files)
                    InStream.Skip(4);

                    long tablePos = InStream.Position();

                    InStream.Seek(fileOff);

                    byte[] FileContents = InStream.ReadBytes(fileLen);
                    //TODO:name table
                    //Helpers.Write(InStream.FilePath, resIDs[r].ToString(), FileContents);
                    byte[] decodedImage = ImgDecompress.DecompressBitmap(FileContents);
                    if (decodedImage == null)
                    {
                        Log.Error("\nUnable to extract Mohawk file");
                        //return false;
                        return true;
                    }
                    Helpers.Write(InStream.FilePath, resIDs[r].ToString() + ".png", decodedImage);

                    InStream.Seek(tablePos);
                }
            }

            return true;
        }
    }
}