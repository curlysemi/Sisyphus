using CommandLine;
using Sisyphus.Commands;
using System.Linq;

namespace Sisyphus
{
    class Program
    {
        public static int Main(string[] args)
        {
            // If no arguments were provided, let's default to the 'help' command
            if (args?.Any() != true)
            {
                args = new[] { "--help" };
            }

            return Parser.Default.ParseArguments<
                Check,
                Dedup,
                Sort,
                MkConf,
                VerDep
                >(args)
            .MapResult(
              (Check cmd) => cmd.Execute(),
              (Dedup cmd) => cmd.Execute(),
              (Sort cmd) => cmd.Execute(),
              (MkConf cmd) => cmd.Execute(),
              (VerDep cmd) => cmd.Execute(),
              errs => 1);
        }
    }
}
