using Sisyphus.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Sisyphus.Helpers
{
    internal class IOExtensions
    {
        public static void Log(string message)
        {
            Console.WriteLine(message);
        }

        public static void NL()
        {
            Console.WriteLine(string.Empty);
        }

        public static void LogNoLine(string message)
        {
            Console.Write(message);
        }

        public static void Vlog(bool isVerbose, string message)
        {
            if (isVerbose)
            {
                Log(message);
            }
        }

        public static void Vt(bool isVerbose, [CallerMemberName] string caller = null)
        {
            if (isVerbose && !string.IsNullOrWhiteSpace(caller))
            {
                Log($"Entered '{caller}'");
            }
        }

        public static void Vt(bool isVerbose, dynamic @params, [CallerMemberName] string caller = null)
        {
            try
            {
                if (isVerbose && !string.IsNullOrWhiteSpace(caller))
                {
                    var formattedParams = "...";
                    if (@params != null)
                    {
                        Type type = @params.GetType();
                        PropertyInfo[] props = type.GetProperties();

                        var printableValues = new List<string>();
                        foreach (var prop in props)
                        {
                            object value = prop.GetValue(@params);

                            string printableValue;
                            if (value is string @string)
                            {
                                printableValue = $"\"{@string}\"";
                            }
                            else
                            {
                                if (value == null)
                                {
                                    printableValue = "null";
                                }
                                else
                                {
                                    var typeOfValue = value.GetType();
                                    if (typeOfValue.IsClass)
                                    {
                                        printableValue = Newtonsoft.Json.JsonConvert.SerializeObject(value);
                                    }
                                    else
                                    {
                                        printableValue = value.ToString();
                                    }
                                }
                            }

                            printableValues.Add(printableValue);
                        }

                        formattedParams = string.Join(", ", printableValues);
                    }
                    Log($"Entered '{caller}({formattedParams})'");
                }
            }
            catch (Exception ex)
            {
                Log("Encountered an exception trying to log an error. (:");
                LogEx(ex, isVerbose);
            }
        }

        public static void LogError(SError error, bool includePrefix = true)
        {
            var colorBefore = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            if (includePrefix)
            {
                Log($"ERROR: {error}");
            }
            else
            {
                Log(error);
            }
            Console.ForegroundColor = colorBefore;
        }

        public static void Warn(SError warning)
        {
            var colorBefore = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Log(warning);
            Console.ForegroundColor = colorBefore;
        }

        public static void LogEx(Exception ex, bool isVerbose)
        {
            var colorBefore = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Log("ERROR: An unexpected error occurred!");
            Log(ex.Message);
            if (isVerbose)
            {
                Console.WriteLine(ex.StackTrace);
            }
            Console.ForegroundColor = colorBefore;
        }
    }
}
