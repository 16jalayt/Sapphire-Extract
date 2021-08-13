using System;

namespace Plugin_Contract
{
    public interface IPlugin
    {
        bool CanExtract();
        void Extract();
    }
}
