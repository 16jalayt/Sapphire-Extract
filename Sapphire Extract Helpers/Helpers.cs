﻿using Serilog;
using System;

namespace Sapphire_Extract_Helpers
{
    public static class Helpers
    {
        public static bool Raw;

        //TODO: pass multiple possible values. Helper to itter and check returns?
        //TODO: better debug messages(pass guessed value to print?)

        /// <summary>
        /// Read a byte array and print if not equal.
        /// </summary>
        /// <param name="InStream"></param>
        /// <param name="val"></param>
        /// <returns>Truth</returns>
        public static bool AssertValue(BetterBinaryReader InStream, byte[] val, string failMessage="")
        {
            byte[] readValues = InStream.ReadBytes(val.Length);
            if (!Equal(readValues, val))
            {
                //TODO:figure out better output. prints int
                Log.Warning($"Value in file {InStream.FileName} at position '{InStream.Position()}'...");
                Log.Warning($"Expected value '{Hex(val)}' got '{Hex(readValues)}'");
                Log.Warning(failMessage);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Read an byte and print if not equal.
        /// </summary>
        /// <param name="InStream"></param>
        /// <param name="val"></param>
        /// <returns>Truth</returns>
        public static bool AssertByte(BetterBinaryReader InStream, int val)
        {
            byte readValue = InStream.ReadByte();
            if (readValue != val)
            {
                //TODO:figure out better output. prints int
                Log.Warning($"Value in file {InStream.FileName} at position '{InStream.Position()}'...");
                Log.Warning($"Expected value '{val}' got '{readValue}'");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Read an int and print if not equal.
        /// </summary>
        /// <param name="InStream"></param>
        /// <param name="val"></param>
        /// <returns>Truth</returns>
        public static bool AssertInt(BetterBinaryReader InStream, int val, bool LittleEndian = true)
        {
            int readValue = 0;
            if (LittleEndian)
                readValue = InStream.ReadInt();
            else
                readValue = InStream.ReadInt();

            if (readValue != val)
            {
                //TODO:figure out better output. prints int
                Log.Warning($"Value in file {InStream.FileName} at position '{InStream.Position() - 4}'...");
                Log.Warning($"Expected value '{val}' got '{readValue}'");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Read an int and print if not equal.
        /// </summary>
        /// <param name="InStream"></param>
        /// <param name="val"></param>
        /// <param name="failMessage"></param>
        /// <returns>Truth</returns>
        public static bool AssertInt(BetterBinaryReader InStream, int val, string failMessage, bool LittleEndian = true)
        {
            bool result = AssertInt(InStream, val, LittleEndian);
            if (!result)
                Log.Warning(failMessage);
            return result;
        }

        /// <summary>
        /// Read a big endian int and print if not equal.
        /// </summary>
        /// <param name="InStream"></param>
        /// <param name="val"></param>
        /// <returns>Truth</returns>
        public static bool AssertIntBE(BetterBinaryReader InStream, int val)
        {
            return AssertInt(InStream, val, false);
        }

        /// <summary>
        /// Read an int and print if not equal.
        /// </summary>
        /// <param name="InStream"></param>
        /// <param name="val"></param>
        /// <returns>Truth</returns>
        public static bool AssertUInt(BetterBinaryReader InStream, uint val, bool LittleEndian = true)
        {
            uint readValue = 0;
            if (LittleEndian)
                readValue = InStream.ReadUInt();
            else
                readValue = InStream.ReadUInt();

            if (readValue != val)
            {
                //TODO:figure out better output. prints int
                Log.Warning($"Value in file {InStream.FileName} at position '{InStream.Position() - 4}'...");
                Log.Warning($"Expected value '{val}' got '{readValue}'");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Read an unsigned big endian int and print if not equal.
        /// </summary>
        /// <param name="InStream"></param>
        /// <param name="val"></param>
        /// <returns>Truth</returns>
        public static bool AssertUIntBE(BetterBinaryReader InStream, ushort val)
        {
            return AssertUInt(InStream, val, false);
        }

        /// <summary>
        /// Read a short and print if not equal.
        /// </summary>
        /// <param name="InStream"></param>
        /// <param name="val"></param>
        /// <returns>Truth</returns>
        public static bool AssertShort(BetterBinaryReader InStream, short val, bool LittleEndian = true)
        {
            short readValue = 0;
            if (LittleEndian)
                readValue = InStream.ReadShort();
            else
                readValue = InStream.ReadShortBE();

            if (readValue != val)
            {
                //TODO:figure out better output. prints int
                Log.Warning($"Value in file {InStream.FileName} at position '{InStream.Position() - 2}'...");
                Log.Warning($"Expected value '{val}' got '{readValue}'");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Read a big endian short and print if not equal.
        /// </summary>
        /// <param name="InStream"></param>
        /// <param name="val"></param>
        /// <returns>Truth</returns>
        public static bool AssertShortBE(BetterBinaryReader InStream, short val)
        {
            return AssertShort(InStream, val, false);
        }

        /// <summary>
        /// Read an unsigned short and print if not equal.
        /// </summary>
        /// <param name="InStream"></param>
        /// <param name="val"></param>
        /// <returns>Truth</returns>
        public static bool AssertUShort(BetterBinaryReader InStream, ushort val, bool LittleEndian = true)
        {
            ushort readValue = 0;
            if (LittleEndian)
                readValue = InStream.ReadUShort();
            else
                readValue = InStream.ReadUShortBE();

            if (readValue != val)
            {
                //TODO:figure out better output. prints int
                Log.Warning($"Value in file {InStream.FileName} at position '{InStream.Position() - 2}'...");
                Log.Warning($"Expected value '{val}' got '{readValue}'");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Read an unsigned big endian short and print if not equal.
        /// </summary>
        /// <param name="InStream"></param>
        /// <param name="val"></param>
        /// <returns>Truth</returns>
        public static bool AssertUShortBE(BetterBinaryReader InStream, ushort val)
        {
            return AssertUShort(InStream, val, false);
        }

        /// <summary>
        /// Read a string and print if not equal.
        /// </summary>
        /// <param name="InStream"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static bool AssertString(BetterBinaryReader InStream, string val, bool TrimEnd = false)
        {
            long position = InStream.Position();
            string readValues = String(InStream.ReadBytes(val.Length));
            if (TrimEnd == true)
                readValues = readValues.TrimEnd();
            //string readValues = String(InStream.ReadBytes(val.Length));
            //Log.Warning(readValues);
            if (readValues != val)
            {
                Log.Warning($"Value in file {InStream.FileName} at position '{position}'...");
                Log.Warning($"Expected value '{val}' got '{readValues}'");
                return false;
            }
            else
                return true;
        }

        /// <summary>
        /// Write a byte array to a specified file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        /// <param name="fileContents"></param>
        /// <param name="subdir"></param>
        public static string Write(string filePath, string fileName, byte[] fileContents, bool subdir = true)
        {
            //It means to interpret the string literally
            //@"\\servername\share\folder"
            //is nicer than this:
            //"\\\\servername\\share\\folder"
            return Writer.WriteFile(@filePath, fileName, fileContents, subdir);
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

        public static int BitRotateLeft(int value, int count)
        {
            return (value << count) | (value >> (8 - count));
        }

        public static int BitRotateRight(int value, int count)
        {
            return (value >> count) | (value << (8 - count));
        }
    }
}