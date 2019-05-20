using CommandLine;
using Newtonsoft.Json;
using Sisyphus.Commands.Base;
using Sisyphus.Core;

using static Sisyphus.Helpers.IOExtensions;

namespace Sisyphus.Commands
{
    [Verb("mkconf", HelpText = "Create a new config file if none exists or print out an existing config")]
    internal class MkConf : BaseCommand
    {
        [Option("with-defaults", Required = false, HelpText = "If creating a new config, create with defaults")]
        public bool WithDefaults { get; set; }

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
