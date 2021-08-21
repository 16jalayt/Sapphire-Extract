using Plugin_Contract;
using Sapphire_Extract_Common;
using Serilog;
using System;
using System.IO;
using System.Linq;

namespace TBV_Dynamix
{
    internal class TBV_Dynamix : IPlugin
    {
        public string Name { get { return "3d Ultra"; } }
        //TODO: make path and file name instanced, calculated on extract?
        //or setup in core, bypassing plugin
        //plugin just passes is file or is archive to printer. doent need to know filename

        public bool init(Serilog.ILogger masterlogger)
        {
            Serilog.Log.Logger = new Serilog.LoggerConfiguration()
          .MinimumLevel.Debug()
          .WriteTo.Logger(masterlogger)
          .CreateLogger();

            return true;
        }

        public bool CanExtract(BinaryReader InStream)
        {
            //TODO: create helper
            InStream.BaseStream.Seek(0, SeekOrigin.Begin);
            byte[] magic = InStream.ReadBytes(8);

            //If the file has wrong id, say we can't extract
            if (System.Text.Encoding.UTF8.GetString(magic) == "TBVolume")
                //if (magic.SequenceEqual(Encoding.UTF8.GetBytes("TBVolume")))
                return true;
            else
                return false;
        }

        public bool Extract(BinaryReader InStream)
        {
            Log.Warning($"Plugin '{Name}' is not finished. Will likely spew out garbage.");
            Log.Warning($"Some files may error with an EOF exception.This appears to be Dynamix's fault.");

            //seek past magic (TBVolume)
            InStream.BaseStream.Seek(9, SeekOrigin.Begin);

            //TODO: assert different
            //unknown. Seems to always stay same. Version?
            InStream.BaseStream.Seek(2, SeekOrigin.Current);
            //# of files (4)
            int NumFiles = InStream.ReadInt32();

            Log.Information($"Extracting {NumFiles} files...");
            //always null?
            InStream.BaseStream.Seek(2, SeekOrigin.Current);
            //Dev's name/email?
            InStream.BaseStream.Seek(24, SeekOrigin.Current);

            //End of entry in file table
            long TableOffset = InStream.BaseStream.Position;

            
            for (int i = 0; i < NumFiles; i++)
            {
                //go back to look up table
                //TODO: move to end?
                InStream.BaseStream.Seek(TableOffset, SeekOrigin.Begin);
                //print unknown chunk for exam. chunk type? Dont think so
                InStream.BaseStream.Seek(4, SeekOrigin.Current);

                //Offset of current file in container
                int FileOffset = InStream.ReadInt32();
                Log.Debug($"File offset: {FileOffset}");
                TableOffset = InStream.BaseStream.Position;

                //go to start of file
                InStream.BaseStream.Seek(FileOffset, SeekOrigin.Begin);
                //name of current output file, and remove \0
                string CurrFileName = System.Text.Encoding.UTF8.GetString(InStream.ReadBytes(24)).Replace("\0", string.Empty);
                Log.Debug($"CurrFiileName: {CurrFileName}");
                
                int Length = InStream.ReadInt32();
                Log.Debug($"File Length: {Length}");
                //System.out.println("lenth:"+ length);

                byte[] FileContents = InStream.ReadBytes(Length);
                //Output.OutSetup(ParseInput.inputWithoutExtension + ParseInput.separator + name, "");
                //ParseInput.outStream.write(fileout);
                System.IO.File.WriteAllBytes(@"C:/Users/16jal/source/repos/Sapphire Extract/Sapphire Extract/bin/Debug/net5.0/out/"+CurrFileName, FileContents);
                Log.Debug($"\n");
            }


            return true;
        }

        public int GetPriority()
        {
            //TODO:simplify with enum?
            return 100;
        }
    }
}
