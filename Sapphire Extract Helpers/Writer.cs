using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sapphire_Extract_Helpers
{
    static class Writer
    {
        public static void WriteFile(string fileName, byte[] fileContents)
        {
            //It means to interpret the string literally
            //@"\\servername\share\folder"
            //is nicer than this:
            //"\\\\servername\\share\\folder"
        }
    }
}
