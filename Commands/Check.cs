using CommandLine;
using Sisyphus.Core;
using System;

namespace Sisyphus.Commands
{
    [Verb("check", HelpText = "Check the provided solution or project file for issues (defaults to whatever is in current directory)")]
    public class Check : BaseCommand
    {
        public override (bool isSuccess, SError error) Run()
        {
            Console.WriteLine("Hello World!");

            // TODO: Actually implement

            return Success;
        }
    }
}
