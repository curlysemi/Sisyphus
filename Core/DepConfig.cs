using Sisyphus.Helpers;
using System;
using System.Linq;
using System.Xml.Linq;

namespace Sisyphus.Core
{
    class DepConfig
    {
        public string Name { get; private set; }

        public string VersionString { get; private set; }

        public Version Version { get; private set; }

        public string TargetFramework { get; private set; }

        public bool IsDevelopmentDependency { get; private set; }

        public string ProjectHintPath(Config config)
        {
            string relativePackagesPath = config?.RelativePackagesPath ?? @"..\packages\";
            return FileHelper.Join(relativePackagesPath, $"{Name}.{VersionString}", "lib", TargetFramework, $"{Name}.dll");
        }

        public DepConfig(XElement packageElement)
        {
            var attrs = packageElement.Attributes();
            if (attrs?.Any() == true)
            {
                foreach (var attr in attrs)
                {
                    switch (attr.Name.LocalName)
                    {
                        case "id":
                            {
                                Name = attr.Value;
                                break;
                            }
                        case "version":
                            {
                                VersionString = attr.Value;
                                try
                                {
                                    if (!VersionString.Contains('-'))
                                    {
                                        Version = new Version(VersionString);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    //LogEx(ex, isVerbose: false);
                                }
                                break;
                            }
                        case "targetFramework":
                            {
                                TargetFramework = attr.Value;
                                break;
                            }
                        case "developmentDependency":
                            {
                                IsDevelopmentDependency = true;
                                break;
                            }
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
        }
    }
}
