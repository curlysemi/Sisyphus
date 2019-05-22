using CommandLine;
using Newtonsoft.Json;
using Sisyphus.Commands.Base;
using Sisyphus.Core;
using System;

using static Sisyphus.Helpers.IOExtensions;

namespace Sisyphus.Commands
{
    [Verb("mkconf", HelpText = "Create a new config file if none exists or print out an existing config.")]
    internal class MkConf : BaseCommand
    {
        [Option("with-defaults", Required = false, HelpText = "If creating a new config, create with defaults (default behavior), else adds defaults to existing config (not default behavior).")]
        public bool? WithDefaults { get; set; }

        protected override bool ShouldTryDefaultConfigLoad => false;

        protected override (bool isSuccess, SError error) Run(Config config)
        {
            void tryPrintConfig()
            {
                if (config != null)
                {
                    Log(JsonConvert.SerializeObject(config, Formatting.Indented));
                }
            }

            tryPrintConfig();

            ConfigPath = ConfigPath ?? Environment.CurrentDirectory;

            if (Config.TryLoadConfigFromPathIfNull(ConfigPath, ref config, createIfNotExist: true, addDefaults: WithDefaults))
            {
                tryPrintConfig();

                return Success;
            }
            else
            {
                return Error();
            }
        }
    }
}
