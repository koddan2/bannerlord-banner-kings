using Serilog;
using System;
using System.IO;

namespace BannerKings._CUSTOM
{
    public static class RobMod
    {
        internal static readonly string _logFile = @"c:\temp\bjorn-bannerlord.txt";
        public static void Init()
        {
            // truncate
            Log("==============================================================================" + Environment.NewLine);
        }

        public static void Log(string message)
        {
            File.AppendAllText(RobMod._logFile, message + Environment.NewLine);
        }
    }
}
