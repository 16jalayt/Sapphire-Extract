using Serilog;
using SevenZipExtractor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sapphire_Extract_Helpers
{
    public static class Compression_Manager
    {
        public static byte[] LZ4DecompressChunk(byte[] input)
        {
            MemoryStream comp = new MemoryStream(input);
            ArchiveFile archiveFile = new ArchiveFile(comp, SevenZipFormat.Lzma);

            MemoryStream decomp = new MemoryStream();
            archiveFile.Entries[0].Extract(decomp);
            return decomp.ToArray();
        }

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


        /*
            * Copyright 2008-2013, David Karnok
            * The file is part of the Open Imperium Galactica project.
            *
            * The code should be distributed under the LGPL license.
            * See http://www.gnu.org/licenses/lgpl.html for details.
            */

        /**
         * Translated from Java
         * Decompress the given byte array using the LZSS algorithm and
         * produce the output into the given out array.
         */

        public static byte[] decompressLZSSDrew(byte[] data)
        {
            int src = 0;
            //int dst = 0;
            List<byte> outdata = new List<byte>();

            int marker = 0;
            int nextChar = 0xFEE;
            int windowSize = 4096;
            byte[] slidingWindow = Enumerable.Repeat((byte)0x20, windowSize).ToArray();

            while (src < data.Length)
            {
                marker = data[src++] & 0xFF;
                for (int i = 0; i < 8 && src < data.Length; i++)
                {
                    bool type = (marker & (1 << i)) != 0;
                    if (type)
                    {
                        byte d = data[src++];
                        outdata.Add(d);
                        slidingWindow[nextChar] = d;
                        nextChar = (nextChar + 1) % windowSize;
                    }
                    else
                    {
                        int offset = data[src++] & 0xFF;
                        int len = data[src++] & 0xFF;
                        offset = offset | (len & 0xF0) << 4;
                        len = (len & 0x0F) + 3;
                        for (int j = 0; j < len; j++)
                        {
                            byte d = slidingWindow[(offset + j) % windowSize];
                            outdata.Add(d);
                            slidingWindow[nextChar] = d;
                            nextChar = (nextChar + 1) % windowSize;
                        }
                    }
                }
            }
            return outdata.ToArray();
        }
    }
}