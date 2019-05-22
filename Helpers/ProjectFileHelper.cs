// Copyright 2015 Kirill Osenkov
// https://github.com/KirillOsenkov/CodeCleanupTools/blob/master/SortProjectItems/SortProjectItems.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Sisyphus.Helpers
{
    public static class ProjectFileHelper
    {
        public static (XDocument document, XElement[] itemGroups) LoadProjectXml(string projectPath)
        {
            XDocument document = XDocument.Load(projectPath, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
            XNamespace msBuildNamespace = document.Root.GetDefaultNamespace();
            XName itemGroupName = XName.Get("ItemGroup", msBuildNamespace.NamespaceName);

            // only consider the top-level item groups, otherwise stuff inside Choose, Targets etc. will be broken
            var itemGroups = document.Root.Elements(itemGroupName).ToArray();

            return (document, itemGroups);
        }

        public static string GetProjectFileParentDirName(string projectPath, out string absoluteProjectFileParentDirPath)
        {
            absoluteProjectFileParentDirPath = FileHelper.GetParentDirectory(projectPath);
            return FileHelper.GetName(absoluteProjectFileParentDirPath);
        }

        public static HashSet<string> GetFilesFromProjectFile(string projectPath, out string projectFileParentDirectoryName)
        {
            projectFileParentDirectoryName = GetProjectFileParentDirName(projectPath, out _);

            var files = new HashSet<string>();

            var (document, itemGroups) = LoadProjectXml(projectPath);

            var fileNodes = new[] { "None", "Compile", "Content", "EmbeddedResource" };

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
