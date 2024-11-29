using Plugin_Contract;
using Sapphire_Extract_Helpers;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;

namespace TPX_LonelyTroops
{
    internal class TPX_LonelyTroops : IPlugin
    {
        /// <summary>
        /// Pretty text that shows in the error logs to identify the plugin.
        /// </summary>
        public string Name
        { get { return "TPX LonelyTroops"; } }

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
            if (Path.GetExtension(InStream.FileName) == ".tpx")
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
            Log.Warning($"Plugin '{Name}' is not finished. Will likely spew out garbage.\n");

            //file len - 8
            int fileLength = InStream.ReadInt("File length: ");
            //version and zero
            InStream.Skip(8);
            int width = InStream.ReadInt("Width: ");
            int height = InStream.ReadInt("Height: ");
            //pixel format info?
            int un1 = InStream.ReadInt("un1: ");
            int un2 = InStream.ReadInt("un2: ");
            InStream.Skip(4);

            byte[] imgData = InStream.ReadBytes((int)(InStream.Length() - InStream.Position()));

            if ((un1 == 28 && un2 == 1) || (un1 == 50 && un2 == 1))
            {
                Image image = Image.LoadPixelData<L8>(imgData, width, height);
                image.Save(InStream.FileNameWithoutExtension + ".png");
            }
            else if (un1 == 23 && un2 == 2)
            {
                Image image = Image.LoadPixelData<Bgr565>(imgData, width, height);
                image.Save(InStream.FileNameWithoutExtension + ".png");
            }
            else if (un1 == 51 && un2 == 2)
            {
                Image image = Image.LoadPixelData<La16>(imgData, width, height);
                image.Save(InStream.FileNameWithoutExtension + ".png");
            }
            else if (un1 == 21 && un2 == 4)
            {
                Image image = Image.LoadPixelData<Bgra32>(imgData, width, height);
                image.Save(InStream.FileNameWithoutExtension + ".png");
            }
            else
            {
                Log.Error("Unknown values");

                //If unknown, try values and add to plugin
                bool exists = Directory.Exists(InStream.FileNameWithoutExtension);
                if (!exists)
                    Directory.CreateDirectory(InStream.FileNameWithoutExtension);

                try
                {
                    Image image = Image.LoadPixelData<Argb32>(imgData, width, height);
                    image.Save(InStream.FileNameWithoutExtension + "/Argb32" + ".png");
                }
                catch { }

                try
                {
                    Image image = Image.LoadPixelData<Bgr565>(imgData, width, height);
                    image.Save(InStream.FileNameWithoutExtension + "/Bgr565" + ".png");
                }
                catch { }

                try
                {
                    Image image = Image.LoadPixelData<Bgra5551>(imgData, width, height);
                    image.Save(InStream.FileNameWithoutExtension + "/Bgra5551" + ".png");
                }
                catch { }

                try
                {
                    Image image = Image.LoadPixelData<Bgr24>(imgData, width, height);
                    image.Save(InStream.FileNameWithoutExtension + "/Bgr24" + ".png");
                }
                catch { }

                try
                {
                    Image image = Image.LoadPixelData<Bgra32>(imgData, width, height);
                    image.Save(InStream.FileNameWithoutExtension + "/Bgra32" + ".png");
                }
                catch { }

                try
                {
                    Image image = Image.LoadPixelData<Abgr32>(imgData, width, height);
                    image.Save(InStream.FileNameWithoutExtension + "/Abgr32" + ".png");
                }
                catch { }

                try
                {
                    Image image = Image.LoadPixelData<L8>(imgData, width, height);
                    image.Save(InStream.FileNameWithoutExtension + "/L8" + ".png");
                }
                catch { }

                try
                {
                    Image image = Image.LoadPixelData<L16>(imgData, width, height);
                    image.Save(InStream.FileNameWithoutExtension + "/L16" + ".png");
                }
                catch { }

                try
                {
                    Image image = Image.LoadPixelData<La16>(imgData, width, height);
                    image.Save(InStream.FileNameWithoutExtension + "/La16" + ".png");
                }
                catch { }

                try
                {
                    Image image = Image.LoadPixelData<La32>(imgData, width, height);
                    image.Save(InStream.FileNameWithoutExtension + "/La32" + ".png");
                }
                catch { }

                try
                {
                    Image image = Image.LoadPixelData<Rgb24>(imgData, width, height);
                    image.Save(InStream.FileNameWithoutExtension + "/Rgb24" + ".png");
                }
                catch { }

                try
                {
                    Image image = Image.LoadPixelData<Rgba32>(imgData, width, height);
                    image.Save(InStream.FileNameWithoutExtension + "/Rgba32" + ".png");
                }
                catch { }

                try
                {
                    Image image = Image.LoadPixelData<Rgb48>(imgData, width, height);
                    image.Save(InStream.FileNameWithoutExtension + "/Rgb48" + ".png");
                }
                catch { }

                try
                {
                    Image image = Image.LoadPixelData<Rgba64>(imgData, width, height);
                    image.Save(InStream.FileNameWithoutExtension + "/Rgba64" + ".png");
                }
                catch { }
            }

            return true;
        }

        public int GetPriority()
        {
            return 100;
        }
    }
}