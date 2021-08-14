using Plugin_Contract;
using System;
using System.IO;

namespace TBV_3D_Ultra
{
    internal class TBV_3D_Ultra : IPlugin
    {
        public string Name { get { return "3d Ultra"; } }

        public bool CanExtract(BinaryReader InStream)
        {
            return true;
        }

        public bool Extract(BinaryReader InStream)
        {
            return true;
        }

        public int GetPriority()
        {
            //TODO:simplify with enum?
            return 100;
        }
    }
}
