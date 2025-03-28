﻿using Sapphire_Extract_Helpers;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MHK_Broderbund
{
    //Code taken from: https://github.com/buildist/Riven-Image-Extractor
    internal static class ImgDecompress
    {
        private static int[] BPP_Lookup = [1, 4, 8, 16, 24];

        public static byte[] DecompressBitmap(byte[] FileContents)
        {
            BetterBinaryReader InStream = new BetterBinaryReader(FileContents, "");

            //only supposed to use last 10 bytes
            int width = InStream.ReadShortBE("Width:") & 0x3ff;
            int height = InStream.ReadShortBE("Height:") & 0x3ff;
            //bpr has to be even
            int bytesPerRow = InStream.ReadShortBE("Bytes per row:") & 0x3fe;

            short isCompressed = InStream.ReadShortBE();
            //wierd bitmasking. 3 values stored in compression value
            int bpp = BPP_Lookup[isCompressed & 0b111];
            if (bpp == 24)
            {
                //TODO:
                Log.Error("Uncompressed not implemented. Report this.");
                /*BufferedImage image = new BufferedImage(width, height, BufferedImage.TYPE_INT_RGB);
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int b = ubyte(bb.get());
                        int g = ubyte(bb.get());
                        int r = ubyte(bb.get());
                        Color color = new Color(r, g, b);
                        image.setRGB(x, y, color.getRGB());
                    }
                    for (int i = 0; i < width; i++)
                    {
                        bb.get();
                    }
                }
                ImageIO.write(image, "png", new FileOutputStream(outputFile));*/
                return [];
            }

            int secondaryCompression = (isCompressed & 0b11110000) >> 4;
            int primaryCompression = (isCompressed & 0b111100000000) >> 8;
            if (primaryCompression != 4 && primaryCompression != 0)
            {
                Log.Error("Unknown primaryCompression " + primaryCompression);
                return null;
            }
            if (secondaryCompression != 0)
            {
                Log.Error("Unknown secondaryCompression " + secondaryCompression);
                return null;
            }

            //unknown short
            if (!Helpers.AssertValue(InStream, [0x03, 0x04]))
            {
                Log.Error("Should be 0x03, 0x04. Report this.");
                return null;
            }

            short bitsPerColor = InStream.ReadByte();
            if (bitsPerColor != 24)
            {
                Log.Error("Unknown bits per color " + bitsPerColor);
                return null;
            }

            int colorCount = InStream.ReadByte() + 1;
            if (colorCount == 0)
                colorCount = 256;
            if (colorCount != 256)
            {
                Log.Error("Nonstandard color count = " + colorCount);
                return null;
            }

            Rgb24[] colors = new Rgb24[colorCount];
            for (int i = 0; i < colorCount; i++)
            {
                byte b = InStream.ReadByte();
                byte g = InStream.ReadByte();
                byte r = InStream.ReadByte();
                //Log.Debug(r + ":" + g + ":" + b);
                colors[i] = Color.FromRgb(r, g, b);
            }
            if (primaryCompression == 0)
            {
                Log.Debug("uncompressed/////////////////////////////////////");
                using (Image<Rgb24> image = new Image<Rgb24>(width, height))
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < bytesPerRow; x++)
                        {
                            int colorIndex = InStream.ReadByte();
                            Rgb24 color = colors[colorIndex];
                            if (x < width)
                                image[x, y] = color;
                        }
                    }
                    using (var ms = new MemoryStream())
                    {
                        image.Save(ms, new PngEncoder());
                        return ms.ToArray();
                    }
                }
            }
            else
            {
                //unknown
                InStream.ReadIntBE();
                int rawLen = (int)(FileContents.Length - InStream.Position());
                Log.Debug("beg data pos: " + InStream.Position());
                //System.out.println(raw.length + ":" + fileOffset + ":" + fileLength + ":" + inStream.getFilePointer());
                byte[] raw = InStream.ReadBytes(rawLen);
                //File.WriteAllBytes("LastImage.raw", raw);
                int[] image = new int[bytesPerRow * height];
                int p = 0;
                int q = 0;

                //raw.length
                while (p < raw.Length)
                {
                    byte cmd = raw[p];
                    p++;

                    if (cmd == 0)
                    {
                        //End of stream
                        break;
                    }
                    else if (cmd <= 0x3f)
                    {
                        // Output n pixel duplets, where n is the command value itself. Pixel data comes
                        // immediately after the command as 2*n bytes representing direct indices in the 8-bit
                        // color table.
                        for (int i = 0; i < cmd; i++)
                        {
                            image[q] = raw[p];
                            image[q + 1] = raw[p + 1];
                            p += 2;
                            q += 2;
                        }
                    }
                    else if (cmd <= 0x7f)
                    {
                        // Repeat last 2 pixels n times, where n = command_value & 0x3F.
                        int pixel1 = image[q - 2];
                        int pixel2 = image[q - 1];
                        for (int i = 0; i < (cmd & 0x3f); i++)
                        {
                            image[q] = pixel1;
                            image[q + 1] = pixel2;
                            q += 2;
                        }
                    }
                    else if (cmd <= 0xbf)
                    {
                        // Repeat last 4 pixels n times, where n = command_value & 0x3F.
                        int pixel1 = image[q - 4];
                        int pixel2 = image[q - 3];
                        int pixel3 = image[q - 2];
                        int pixel4 = image[q - 1];
                        for (int i = 0; i < (cmd & 0x3f); i++)
                        {
                            image[q] = pixel1;
                            image[q + 1] = pixel2;
                            image[q + 2] = pixel3;
                            image[q + 3] = pixel4;
                            q += 4;
                        }
                    }
                    else
                    {
                        // Begin of a subcommand stream. This is like the main command stream, but contains
                        // another set of commands which are somewhat more specific and a bit more complex.
                        // This command says that command_value & 0x3F subcommands will follow.
                        int subCount = cmd & 0x3f;
                        for (int i = 0; i < subCount; i++)
                        {
                            int sub = raw[p];
                            //System.out.println(p + ":" + cmd + ":" + sub + ":" + subCount);
                            p++;
                            if (sub >= 0x01 && sub <= 0x0f)
                            {
                                // 0000mmmm
                                // Repeat duplet at relative position -m, where m is given in duplets. So if m=1,
                                // repeat the last duplet.
                                int offset = -(sub & 0b00001111) * 2;
                                image[q] = image[q + offset];
                                image[q + 1] = image[q + offset + 1];
                                q += 2;
                            }
                            else if (sub == 0x10)
                            {
                                // Repeat last duplet, but change second pixel to p.
                                image[q] = image[q - 2];
                                image[q + 1] = raw[p];
                                p++;
                                q += 2;
                            }
                            else if (sub >= 0x11 && sub <= 0x1f)
                            {
                                // 0001mmmm
                                // Output the first pixel of last duplet, then pixel at relative position -m. m is
                                // given in pixels. (relative to the second pixel!)
                                int offset = -(sub & 0b00001111) + 1;
                                image[q] = image[q - 2];
                                image[q + 1] = image[q + offset];
                                q += 2;
                            }
                            else if (sub >= 0x20 && sub <= 0x2f)
                            {
                                // 0010xxxx
                                // Repeat last duplet, but add x to second pixel.
                                image[q] = image[q - 2];
                                image[q + 1] = image[q - 1] + (sub & 0b00001111);
                                q += 2;
                            }
                            else if (sub >= 0x30 && sub <= 0x3f)
                            {
                                // 0011xxxx
                                // Repeat last duplet, but subtract x from second pixel.
                                image[q] = image[q - 2];
                                image[q + 1] = image[q - 1] - (sub & 0b00001111);
                                q += 2;
                            }
                            else if (sub == 0x40)
                            {
                                // Repeat last duplet, but change first pixel to p.
                                image[q] = raw[p];
                                image[q + 1] = image[q - 1];
                                p++;
                                q += 2;
                            }
                            else if (sub >= 0x41 && sub <= 0x4f)
                            {
                                // 0100mmmm
                                // Output pixel at relative position -m, then second pixel of last duplet.
                                int offset = -(sub & 0b00001111);
                                image[q] = image[q + offset];
                                image[q + 1] = image[q - 1];
                                q += 2;
                            }
                            else if (sub == 0x50)
                            {
                                // Output two absolute pixel values, p1 and p2.
                                image[q] = raw[p];
                                image[q + 1] = raw[p + 1];
                                p += 2;
                                q += 2;
                            }
                            else if (sub >= 0x51 && sub <= 0x57)
                            {
                                // 01010mmm p
                                // Output pixel at relative position -m, then absolute pixel value p.
                                int offset = -(sub & 0b00000111);
                                image[q] = image[q + offset];
                                image[q + 1] = raw[p];
                                p++;
                                q += 2;
                            }
                            else if (sub >= 0x59 && sub <= 0x5f)
                            {
                                // 01011mmm p
                                // Output absolute pixel value p, then pixel at relative position -m.
                                // (relative to the second pixel!)
                                int offset = -(sub & 0b00000111) + 1;
                                image[q] = raw[p];
                                image[q + 1] = image[q + offset];
                                p++;
                                q += 2;
                            }
                            else if (sub >= 0x60 && sub <= 0x6f)
                            {
                                // 0110xxxx p
                                // Output absolute pixel value p, then (second pixel of last duplet) + x.
                                image[q] = raw[p];
                                image[q + 1] = image[q - 1] + (sub & 0b00001111);
                                p++;
                                q += 2;
                            }
                            else if (sub >= 0x70 && sub <= 0x7f)
                            {
                                // 0111xxxx p
                                // Output absolute pixel value p, then (second pixel of last duplet) - x.
                                image[q] = raw[p];
                                image[q + 1] = image[q - 1] - (sub & 0b00001111);
                                p++;
                                q += 2;
                            }
                            else if (sub >= 0x80 && sub <= 0x8f)
                            {
                                // 1000xxxx
                                // Repeat last duplet adding x to the first pixel.
                                image[q] = image[q - 2] + (sub & 0b00001111);
                                image[q + 1] = image[q - 1];
                                q += 2;
                            }
                            else if (sub >= 0x90 && sub <= 0x9f)
                            {
                                // 1001xxxx p
                                // Output (first pixel of last duplet) + x, then absolute pixel value p.
                                image[q] = image[q - 2] + (sub & 0b00001111);
                                image[q + 1] = raw[p];
                                p++;
                                q += 2;
                            }
                            else if (sub == 0xa0)
                            {
                                // 0xa0 xxxxyyyy
                                // Repeat last duplet, adding x to the first pixel and y to the second.
                                int x = (raw[p] & 0b11110000) >> 4;
                                int y = raw[p] & 0b00001111;
                                image[q] = image[q - 2] + x;
                                image[q + 1] = image[q - 1] + y;
                                p++;
                                q += 2;
                            }
                            else if (sub == 0xb0)
                            {
                                // 0xb0 xxxxyyyy
                                // Repeat last duplet, adding x to the first pixel and subtracting y to the
                                // second.
                                int x = (raw[p] & 0b11110000) >> 4;
                                int y = raw[p] & 0b00001111;
                                image[q] = image[q - 2] + x;
                                image[q + 1] = image[q - 1] - y;
                                p++;
                                q += 2;
                            }
                            else if (sub >= 0xc0 && sub <= 0xcf)
                            {
                                // 1100xxxx
                                // Repeat last duplet subtracting x from first pixel.
                                image[q] = image[q - 2] - (sub & 0b00001111);
                                image[q + 1] = image[q - 1];
                                q += 2;
                            }
                            else if (sub >= 0xd0 && sub <= 0xdf)
                            {
                                // 1101xxxx p
                                // Output (first pixel of last duplet) - x, then absolute pixel value p.
                                image[q] = image[q - 2] - (sub & 0b00001111);
                                image[q + 1] = raw[p];
                                p++;
                                q += 2;
                            }
                            else if (sub == 0xe0)
                            {
                                // 0xe0 xxxxyyyy
                                // Repeat last duplet, subtracting x from first pixel and adding y to second.
                                int x = (raw[p] & 0b11110000) >> 4;
                                int y = raw[p] & 0b00001111;
                                image[q] = image[q - 2] - x;
                                image[q + 1] = image[q - 1] + y;
                                p++;
                                q += 2;
                            }
                            else if (sub == 0xf0 || sub == 0xff)
                            {
                                // 0xfx xxxxyyyy
                                // Repeat last duplet, subtracting x from first pixel and y from second.
                                int x = ((sub & 0b00001111) << 4) | ((raw[p] & 0b11110000) >> 4);
                                int y = raw[p] & 0b00001111;
                                image[q] = image[q - 2] - x;
                                image[q + 1] = image[q - 1] - y;
                                p++;
                                q += 2;
                            }
                            else if ((sub & 0b10100000) == 0b10100000 && sub != 0xfc)
                            {
                                // 1x1xxxmm mmmmmmmm
                                // Repeat n duplets from relative position -m (given in pixels, not duplets). If r
                                // is 0, another byte follows and the last pixel is set to that value. n and r come
                                // from the table on the right.
                                int n = 0;
                                int r = 0;
                                if (sub >= 0xa4 && sub <= 0xa7)
                                {
                                    n = 2;
                                    r = 0;
                                }
                                else if (sub >= 0xa8 && sub <= 0xab)
                                {
                                    n = 2;
                                    r = 1;
                                }
                                else if (sub >= 0xac && sub <= 0xaf)
                                {
                                    n = 3;
                                    r = 0;
                                }
                                else if (sub >= 0xb4 && sub <= 0xb7)
                                {
                                    n = 3;
                                    r = 1;
                                }
                                else if (sub >= 0xb8 && sub <= 0xbb)
                                {
                                    n = 4;
                                    r = 0;
                                }
                                else if (sub >= 0xbc && sub <= 0xbf)
                                {
                                    n = 4;
                                    r = 1;
                                }
                                else if (sub >= 0xe4 && sub <= 0xe7)
                                {
                                    n = 5;
                                    r = 0;
                                }
                                else if (sub >= 0xe8 && sub <= 0xeb)
                                {
                                    n = 5;
                                    r = 1;
                                }
                                else if (sub >= 0xec && sub <= 0xef)
                                {
                                    n = 6;
                                    r = 0;
                                }
                                else if (sub >= 0xf4 && sub <= 0xf7)
                                {
                                    n = 6;
                                    r = 1;
                                }
                                else if (sub >= 0xf8 && sub <= 0xfb)
                                {
                                    n = 7;
                                    r = 0;
                                }
                                else
                                {
                                    Log.Error("subcommand: " + sub);
                                }

                                int offset = -(raw[p] | ((sub & 0b00000011) << 8));
                                p++;
                                for (int j = 0; j < n; j++)
                                {
                                    image[q + 2 * j] = image[q + offset + 2 * j];
                                    image[q + 2 * j + 1] = image[q + offset + 2 * j + 1];
                                }
                                q += 2 * n;
                                if (r == 0)
                                {
                                    image[q - 1] = raw[p];
                                    p++;
                                }
                            }
                            else if (sub == 0xfc)
                            {
                                // 0xfc nnnnnrmm mmmmmmmm (p)
                                // Repeat n+2 duplets from relative position -m (given in pixels, not duplets). If
                                // r is 0, another byte p follows and the last pixel is set to absolute value p.
                                int n = (raw[p] & 0b11111000) >> 3;
                                int r = (raw[p] & 0b00000100) >> 2;
                                int offset = -(raw[p + 1] | ((raw[p] & 0b00000011) << 8));

                                for (int j = 0; j < n + 2; j++)
                                {
                                    image[q + 2 * j] = image[q + offset + 2 * j];
                                    image[q + 2 * j + 1] = image[q + offset + 2 * j + 1];
                                }
                                p += 2;
                                q += 2 * n + 4;
                                if (r == 0)
                                {
                                    image[q - 1] = raw[p];
                                    p++;
                                }
                            }
                            else
                            {
                                Log.Error("subcommand2: " + sub);
                            }
                        }
                    }
                }
                using (Image<Rgb24> uncompressedImage = new Image<Rgb24>(width, height))
                {
                    int i = 0;
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < bytesPerRow; x++)
                        {
                            int colorIndex = image[i];
                            if (colorIndex < 0)
                                colorIndex = 255;
                            Rgb24 color = colors[colorIndex & 0xff];
                            if (x < width)
                                uncompressedImage[x, y] = color;
                            i++;
                        }
                    }
                    using (var ms = new MemoryStream())
                    {
                        uncompressedImage.Save(ms, new PngEncoder());
                        return ms.ToArray();
                    }
                }
            }
        }
    }
}