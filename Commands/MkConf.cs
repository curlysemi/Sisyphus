using CommandLine;
using Newtonsoft.Json;
using Sisyphus.Commands.Base;
using Sisyphus.Core;

namespace Sisyphus.Commands
{
    [Verb("mkconf", HelpText = "Create a new config file if none exists or print out an existing config")]
    internal class MkConf : BaseCommand
    {
        protected override (bool isSuccess, SError error) Run(Config config)
        {
            if (TryLoadConfigFromPathIfNull(ConfigPath, ref config, createIfNotExist: true))
            {
                Log(JsonConvert.SerializeObject(config, Formatting.Indented));

                return Success;
            }
            else
            {
                return Error();
            }
        }
    }
}
