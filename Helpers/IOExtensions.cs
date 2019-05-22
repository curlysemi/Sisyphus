using Sisyphus.Core;
using System;

namespace Sisyphus.Helpers
{
    internal class IOExtensions
    {
        public static void Log(string message)
        {
            Console.WriteLine(message);
        }

        public static void NL()
        {
            Console.WriteLine(string.Empty);
        }

        public static void LogNoLine(string message)
        {
            Console.Write(message);
        }

        public static void Vlog(bool isVerbose, string message)
        {
            if (isVerbose)
            {
                Log(message);
            }
        }

        public static void LogError(SError error)
        {
            var colorBefore = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Log(error);
            Console.ForegroundColor = colorBefore;
        }

        public static void Warn(SError warning)
        {
            var colorBefore = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Log(warning);
            Console.ForegroundColor = colorBefore;
        }

        public static void LogEx(Exception ex, bool isVerbose)
        {
            var colorBefore = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Log("An unexpected error occurred!");
            Log(ex.Message);
            if (isVerbose)
            {
                Console.WriteLine(ex.StackTrace);
            }
            Console.ForegroundColor = colorBefore;
        }
    }
}
