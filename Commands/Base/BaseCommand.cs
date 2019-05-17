using CommandLine;
using Newtonsoft.Json;
using Sisyphus.Core;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Sisyphus.Commands.Base
{
    internal abstract class BaseCommand
    {
        [Option('v', "verbose", Required = false, HelpText = "Run with verbose logging.")]
        public bool Verbose { get; set; }

        [Option('c', "config", Required = false, HelpText = "Set the path for the 'Sisyphus.js' configuation file")]
        public string ConfigPath { get; set; }

        public static (bool isSuccess, SError error) Success => (true, null);
        public static (bool isSuccess, SError error) Error(SError error = null) => (false, error);

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

        protected void Warn(SError warning)
        {
            var colorBefore = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Log(warning);
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

        private bool TryLoadConfigFromPath(string path, out Config config)
        {
            config = null;
            return TryLoadConfigFromPathIfNull(path, ref config);
        }

        private string StripComments(string src)
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

        protected virtual bool TryLoadConfigFromPathIfNull(string path, ref Config config, bool createIfNotExist = false)
        {
            if (config == null)
            {
                if (!string.IsNullOrWhiteSpace(path))
                {
                    var filePath = path;
                    if (!path.EndsWith(Constants.SISYPHUS_CONFIG_FILENAME))
                    {
                        filePath = Path.Join(path, Constants.SISYPHUS_CONFIG_FILENAME);
                    }
                    if (File.Exists(filePath))
                    {
                        var configSrc = File.ReadAllText(filePath);
                        configSrc = StripComments(configSrc);
                        try
                        {
                            config = JsonConvert.DeserializeObject<Config>(configSrc);
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

                    if (config == null && createIfNotExist)
                    {
                        config = new Config();
                        Log($"Saving new config!");
                        File.WriteAllText(filePath, JsonConvert.SerializeObject(config, Formatting.Indented));
                    }
                }

                return config != null;
            }
            else
            {
                // a config was already loaded
                Warn($"WARNING: A configuration file was already loaded and '{path}' was not checked!");
                return false;
            }
        }

        protected virtual void PreRunSetup(ref Config config) { }

        public virtual int Execute()
        {
            try
            {
                Vlog("Verbose logging enabled!");

                TryLoadConfigFromPath(ConfigPath, out Config config);

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
