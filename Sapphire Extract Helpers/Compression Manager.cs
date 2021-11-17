using Serilog;
using SevenZipExtractor;
using System.IO;

namespace Sapphire_Extract_Helpers
{
    public static class Compression_Manager
    {
        public static byte[] LZ4Decompress(byte[] input)
        {
            /*InitNativeLibrary();

            LZ4FrameDecompressOptions decompOpts = new LZ4FrameDecompressOptions();

            using (MemoryStream comp = new MemoryStream(input))
            using (MemoryStream decomp = new MemoryStream())
            using (LZ4FrameStream zs = new LZ4FrameStream(comp, decompOpts))
            {
                zs.CopyTo(decomp);
                return decomp.ToArray();
            }*/
            using (MemoryStream comp = new MemoryStream(input))
            using (ArchiveFile archiveFile = new ArchiveFile(comp, SevenZipFormat.Lzma))
            {
                //TODO: refactor? will only trigger once
                foreach (Entry entry in archiveFile.Entries)
                {
                    if (entry.FileName != null)
                        Log.Debug($"Archive File Name: {entry.FileName}");

                    MemoryStream decomp = new MemoryStream();
                    entry.Extract(decomp);
                    return decomp.ToArray();
                }
            }
            return null;
        }
    }
}