using DotNet.Globbing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Sisyphus.Helpers;

using static Sisyphus.Helpers.IOExtensions;
using System.Linq;

namespace Sisyphus.Core
{
    [Serializable]
    public class Config
    {
        private string ConfigPath { get; set; }

        public HashSet<string> IgnorableFiles { get; set; } = new HashSet<string>();

        public static void ApplyDefaults(ref Config config)
        {
            config.IgnorableFiles.AddRange(
                "**.gitignore",
                "**.tfignore",
                "**.excludes"
            );
        }

        public bool IsIgnorable(string filePath)
        {
            if (IgnorableFiles?.Any() == true)
            {
                foreach (var ignorable in IgnorableFiles)
                {
                    var glob = Glob.Parse(ignorable);
                    var isMatch = glob.IsMatch(filePath);
                    if (isMatch)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool TryLoadConfig(string configPath, out Config config)
        {
            config = null;
            if (!string.IsNullOrWhiteSpace(configPath))
            {
                var filePath = configPath;
                if (!configPath.EndsWith(Constants.SISYPHUS_CONFIG_FILENAME))
                {
                    filePath = Path.Join(configPath, Constants.SISYPHUS_CONFIG_FILENAME);
                }
                if (File.Exists(filePath))
                {
                    var configSrc = File.ReadAllText(filePath);
                    configSrc = StripComments(configSrc);
                    try
                    {
                        config = JsonConvert.DeserializeObject<Config>(configSrc);
                        config.ConfigPath = filePath;
                    }
                    catch (Exception ex)
                    {
                        LogError($"An error occurred when trying to read '{filePath}'. Please ensure '{Constants.SISYPHUS_CONFIG_FILENAME}' is correct JSON.");
                        throw ex;
                    }
                }
                else
                {
                    LogNoLine($"'{filePath}' does not exist! ");
                }
            }

            return config != null;
        }

        public static bool TryLoadConfigFromPathIfNull(string path, ref Config config, bool createIfNotExist = false, bool addDefaults = false, bool suppressWarning = false)
        {
            if (config == null || addDefaults)
            {
                bool wasCreated;
                if (wasCreated = config == null && !TryLoadConfig(path, out config) && createIfNotExist)
                {
                    config = new Config();
                }

                if (addDefaults)
                {
                    if (!createIfNotExist)
                    {
                        Warn($"WARNING: Any comments in config at '{path}' will be lost");
                    }

                    ApplyDefaults(ref config);
                }

                // Save
                if (wasCreated || addDefaults)
                {
                    config.Save();
                }

                return config != null;
            }
            else
            {
                // a config was already loaded
                if (!suppressWarning)
                {
                    Warn($"WARNING: A configuration file was already loaded and any possible config at '{path}' was not checked!");
                }
                return false;
            }
        }

        public void Save()
        {
            Log($"Saving new config!");
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        private static string StripComments(string src)
        {
            var blockComments = @"/\*(.*?)\*/";
            var lineComments = @"//(.*?)\r?\n";
            var strings = @"""((\\[^\n]|[^""\n])*)""";
            var verbatimStrings = @"@(""[^""]*"")+";

            return Regex.Replace(src, $"{blockComments}|{lineComments}|{strings}|{verbatimStrings}",
                e => {
                    bool lineCmt = e.Value.StartsWith("//");
                    if (lineCmt || e.Value.StartsWith("/*"))
                    {
                        return lineCmt ? Environment.NewLine : string.Empty;
                    }
                    else
                    {
                        return e.Value;
                    }
                },
                RegexOptions.Singleline
            );
        }
    }
}
