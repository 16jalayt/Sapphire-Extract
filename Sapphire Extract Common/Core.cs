using System;

namespace Sapphire_Extract_Common
{
/// <summary>
/// Common functions between the CLI and GUI
/// </summary>
    public class Core
    {
        //parse cmd line. go strait to extraction on gui? or set overwrite/etc?
        //make cli class called Opts ie Opts.overwrite

        //have variable input files. core should take in list or single file name and marshal 
        //  the (later) threads and trigger the extraction events (also in core)





        /// <summary>
        /// Parse CLI arguments and init plugins.
        /// </summary>
        /// <param name="input">Temporary test param</param>
        public static void init(string input)
        {
            Console.WriteLine(input);
            string[] args = Environment.GetCommandLineArgs();
            foreach(string arg in args)
            {
                Console.WriteLine(arg);
            }
            
        }
    }
}