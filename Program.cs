using CommandLine;
using Sisyphus.Commands;
using System;
using System.Linq;

namespace Sisyphus
{
    class Program
    {
        public static int Main(string[] args)
        {
            if (args?.Any() != true)
            {
                args = new[] { "--help" };
            }

            return Parser.Default.ParseArguments<
                Check,
                Sort
                >(args)
            .MapResult(
              (Check cmd) => cmd.Execute(),
              (Sort cmd) => cmd.Execute(),
              errs => 1);
        }
    }
}
