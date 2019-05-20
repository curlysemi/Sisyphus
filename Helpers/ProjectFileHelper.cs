using ByteDev.DotNet.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Sisyphus.Helpers
{
    public static class ProjectFileHelper
    {
        public static KeyValuePair<string, string>? GetNamePathMappingFromProjectFile(string projectFilePath)
        {
            // ByteDev.DotNet.Project doesn't provide access to the includes :/

            var project = DotNetProject.Load(projectFilePath);

            return null;
        }

        public static HashSet<string> GetFilesFromProjectFile(string projectPath, out string projectFileParentDirectoryName)
        {
            var absoluteProjectFileParentDirPath = FileHelper.GetParentDirectory(projectPath);
            projectFileParentDirectoryName = FileHelper.GetName(absoluteProjectFileParentDirPath);

            var files = new HashSet<string>();

            XDocument document = XDocument.Load(projectPath, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
            XNamespace msBuildNamespace = document.Root.GetDefaultNamespace();
            XName itemGroupName = XName.Get("ItemGroup", msBuildNamespace.NamespaceName);

            // only consider the top-level item groups, otherwise stuff inside Choose, Targets etc. will be broken
            var itemGroups = document.Root.Elements(itemGroupName).ToArray();

            var fileNodes = new[] { "None", "Compile", "Content" };

            foreach (XElement itemGroup in itemGroups)
            {
                var visited = new HashSet<string>();
                var original = itemGroup.Elements().ToArray();
                foreach (var item in original)
                {
                    if (fileNodes.Contains(item.Name.LocalName))
                    {
                        var includePath = item.Attributes()?.FirstOrDefault(a => a.Name.LocalName == "Include")?.Value;
                        if (!string.IsNullOrWhiteSpace(includePath))
                        {
                            var file = $"{projectFileParentDirectoryName}/{includePath.Replace("\\", "/")}";

                            var unescapedFile = Uri.UnescapeDataString(file);

                            files.Add(unescapedFile);
                        }
                    }
                }
            }

            return files;
        }
    }
}
