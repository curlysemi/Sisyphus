using CommandLine;
using Newtonsoft.Json;
using Sisyphus.Core;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Sisyphus.Commands.Base
{
    public abstract class BaseCommand
    {
        [Option('v', "verbose", Required = false, HelpText = "Run with verbose logging.")]
        public bool Verbose { get; set; }

        public static (bool isSuccess, SError error) Success => (true, null);
        public static (bool isSuccess, SError error) Error(SError error) => (false, error);

        public void Log(string message)
        {
            Console.WriteLine(message);
        }

        public void LogNoLine(string message)
        {
            Console.Write(message);
        }

        public void Vlog(string message)
        {
            if (Verbose)
            {
                Log(message);
            }
        }

        //public void LogLines(params string[] lines)
        //{
        //    Console.Write()
        //}

        protected void LogError(SError error)
        {
            var colorBefore = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Log(error);
            Console.ForegroundColor = colorBefore;
        }

        private void LogEx(Exception ex)
        {
            var colorBefore = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Log("An unexpected error occurred!");
            Log(ex.Message);
            if (Verbose)
            {
                Console.WriteLine(ex.StackTrace);
            }
            Console.ForegroundColor = colorBefore;
        }

        public virtual int Execute()
        {
            try
            {
                Vlog("Verbose logging enabled!");

                var (isSuccess, error) = Run();

                if (isSuccess)
                {
                    if (error != null)
                    {
                        Log("WARNING: Error occurred, even though execution was apparently successful!");
                        LogError(error);
                    }
                    return 0;
                }
                else
                {
                    LogError(error ?? new SError());
                }
            }
            catch (Exception ex)
            {
                LogEx(ex);
            }

            return 1;
        }

        protected abstract (bool isSuccess, SError error) Run();
    }
}
