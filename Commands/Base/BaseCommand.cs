using CommandLine;
using Sisyphus.Core;
using System;
using System.Runtime.CompilerServices;
using static Sisyphus.Helpers.IOExtensions;

namespace Sisyphus.Commands.Base
{
    internal abstract class BaseCommand
    {
        [Option('v', "verbose", Required = false, HelpText = "Run with verbose logging.")]
        public bool IsVerbose { get; set; }

        [Option('c', "config", Required = false, HelpText = "Set the path for the 'Sisyphus.js' configuration file (commands that take an input path argument will check for a nearby config file by default).")]
        public string ConfigPath { get; set; }

        public static (bool isSuccess, SError error) Success => (true, null);
        public static (bool isSuccess, SError error) Error(SError error = null) => (false, error);

        protected void Vlog(string message) => Helpers.IOExtensions.Vlog(IsVerbose, message);

        protected void LogEx(Exception ex) => Helpers.IOExtensions.LogEx(ex, IsVerbose);

        protected void Vt([CallerMemberName] string caller = null) => Helpers.IOExtensions.Vt(IsVerbose, caller);

        protected void Vt(dynamic @params, [CallerMemberName] string caller = null) => Helpers.IOExtensions.Vt(IsVerbose, @params, caller);

        protected virtual (bool isSuccess, SError error) PreRunSetup(ref Config config) => Success;

        protected virtual bool ShouldTryDefaultConfigLoad { get; set; } = true;

        public virtual int Execute()
        {
            try
            {
                Program.IsVerbose = IsVerbose;

                Log("------ sisyphus started ------");
                Vlog("Verbose logging enabled!");
                Vlog("\n");

                Config config = null;
                if (ShouldTryDefaultConfigLoad)
                {
                    Config.TryLoadConfig(ConfigPath, out config);
                }

                PreRunSetup(ref config);

                var (isSuccess, error) = Run(config);

                if (isSuccess)
                {
                    if (error != null)
                    {
                        Warn("WARNING: Error occurred, even though execution was apparently successful!");
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

        protected abstract (bool isSuccess, SError error) Run(Config config);
    }
}
