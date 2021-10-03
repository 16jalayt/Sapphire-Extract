using Plugin_Contract;
using Sapphire_Extract_Helpers;
using Serilog;

namespace TBV_Dynamix
{
    //Trophy Bass Volume
    internal class TBV_Dynamix : IPlugin
    {
        public string Name { get { return "3d Ultra"; } }
        //TODO: make path and file name instanced, calculated on extract?
        //or setup in core, bypassing plugin
        //plugin just passes is file or is archive to printer. doent need to know filename

        public bool Init(Serilog.ILogger masterlogger)
        {
            Serilog.Log.Logger = new Serilog.LoggerConfiguration()
          .MinimumLevel.Debug()
          .WriteTo.Logger(masterlogger)
          .CreateLogger();

            return true;
        }

        public bool CanExtract(BetterBinaryReader InStream)
        {
            InStream.Seek(0);
            byte[] magic = InStream.ReadBytes(8);

            //If the file has wrong id, say we can't extract
            if (Helpers.String(magic) == "TBVolume")
                return true;
            else
                return false;
        }

        public bool Extract(BetterBinaryReader InStream)
        {
            Log.Warning($"Plugin '{Name}' is not finished. Will likely spew out garbage.");
            Log.Warning($"Some files may error with an EOF exception.This appears to be Dynamix's fault.");

            //seek past magic (TBVolume)
            InStream.Seek(9);

            //TODO: assert different
            //unknown. Seems to always stay same. Version?
            Helpers.AssertValue(InStream, new byte[] { 0xD0, 0x07 });
            //InStream.Skip(2);
            //# of files (4)
            int NumFiles = InStream.ReadInt();

            Log.Information($"Extracting {NumFiles} files...");
            //always null?
            InStream.Skip(2);
            //Dev's name/email?
            InStream.Skip(24);

            //End of entry in file table
            long TableOffset = InStream.Position();

            for (int i = 0; i < NumFiles; i++)
            {
                //go back to look up table
                //TODO: move to end?
                InStream.Seek(TableOffset);
                //print unknown chunk for exam. chunk type? Dont think so
                InStream.Skip(4);

                //Offset of current file in container
                int FileOffset = InStream.ReadInt();
                Log.Debug($"File offset: {FileOffset}");
                TableOffset = InStream.Position();

                //go to start of file
                InStream.Seek(FileOffset);
                //name of current output file, and remove \0
                string CurrFileName = Helpers.String(InStream.ReadBytes(24)).Replace("\0", string.Empty);
                Log.Debug($"CurrFiileName: {CurrFileName}");

                int Length = InStream.ReadInt();
                Log.Debug($"File Length: {Length}");
                //System.out.println("lenth:"+ length);

                byte[] FileContents = InStream.ReadBytes(Length);
                //Output.OutSetup(ParseInput.inputWithoutExtension + ParseInput.separator + name, "");
                //ParseInput.outStream.write(fileout);
                System.IO.File.WriteAllBytes(@"C:/Users/16jal/source/repos/Sapphire Extract/Sapphire Extract/bin/Debug/net5.0/out/" + CurrFileName, FileContents);
                Log.Debug($"\n");
            }

            return true;
        }

        public int GetPriority()
        {
            //TODO:simplify with enum?
            return 100;
        }

        /*public void Cleanup()
        {
        }*/
    }
}