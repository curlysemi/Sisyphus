using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;
using SError = Sisyphus.Core.SError;

namespace Sisyphus.Commands
{
    public abstract class BaseCommand
    {
        [Option('v', "verbose", Required = false, HelpText = "Run with verbose logging.")]
        public bool Verbose { get; set; }

        public static (bool isSuccess, SError error) Success => (true, null);
        public static (bool isSuccess, SError error) Error(SError error) => (false, error);

        private void PrintError(SError error, bool warn = false)
        {
            if (warn)
            {
                Console.WriteLine("WARNING: Error occurred, even though execution was apparently successful!");
            }
            Console.WriteLine(error);
        }

        public void Vlog(string message)
        {
            if (Verbose)
            {
                Console.WriteLine(message);
            }
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
                        PrintError(error, warn: true);
                    }
                    return 0;
                }
                else
                {
                    PrintError(error ?? new SError());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An unexpected error occurred!");
                if (Verbose)
                {
                    Console.WriteLine(ex.StackTrace);
                }
            }

            return 1;
        }

        public abstract (bool isSuccess, SError error) Run();
    }
}
