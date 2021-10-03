using Serilog;
using System;

namespace Sapphire_Extract_Helpers
{
    public static class Helpers
    {
        //TODO:interface like assert for validating versions in files
        //TODO: wrapper for binary reader?

        public static bool Raw;

        //TODO: pass multiple possible values. Helper to itter and check returns?
        //TODO: better debug messages(pass guessed value to print?)
        public static bool AssertValue(BetterBinaryReader InStream, byte[] val)
        {
            byte[] readValues = InStream.ReadBytes(val.Length);
            if (!Equal(readValues, val))
            {
                //TODO:figure out better output. prints int
                Log.Warning($"Value in file {InStream.FileName()} at position '{InStream.Position()}'...");
                Log.Warning($"Expected value '{Hex(val)}' got '{Hex(readValues)}'");
                return false;
            }
            return true;
        }

        public static string Hex(byte[] inArray)
        {
            return BitConverter.ToString(inArray).Replace("-", ", ");
        }
        public static string String(byte[] inArray)
        {
            return System.Text.Encoding.UTF8.GetString(inArray);
        }

        //https://stackoverflow.com/questions/18472867/checking-equality-for-two-byte-arrays/18472958
        public static bool Equal(byte[] a1, byte[] b1)
        {
            // If not same length, done
            if (a1.Length != b1.Length)
            {
                return false;
            }

            // If they are the same object, done
            if (object.ReferenceEquals(a1, b1))
            {
                return true;
            }

            // Loop all values and compare
            for (int i = 0; i < a1.Length; i++)
            {
                if (a1[i] != b1[i])
                {
                    return false;
                }
            }

            // If we got here, equal
            return true;
        }

        public static void AssertString(BetterBinaryReader InStream, byte[] val)
        {
            //byte[] readValues = InStream.ReadBytes(val.Length);
        }

        //may not use
        public static void AssertValueAbort(byte[] val)
        {
        }

        //Pass off to different class
        public static void Write(string filePath, string fileName, byte[] fileContents, bool subdir = true)
        {
            //It means to interpret the string literally
            //@"\\servername\share\folder"
            //is nicer than this:
            //"\\\\servername\\share\\folder"
            Writer.WriteFile(@filePath, fileName, fileContents, subdir);
        }
        public static void setOverwriteAll(bool val)
        {
            Writer.OverwriteAll = val;
        }
        public static void setAutoRename(bool val)
        {
            Writer.AutoRename = val;
        }
        public static void setRaw(bool val)
        {
            Raw = val;
        }
    }
}