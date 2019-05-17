using System;
using System.Collections.Generic;
using System.Text;
using LibGit2Sharp;
using System.Linq;
using System.Xml.Linq;
using Sisyphus.Core.Enums;
using System.IO;

namespace Sisyphus.Helpers
{
    public static class FileHelper
    {
        public static string GetParentDirectory(string path)
        {
            var parentDirectory = Path.GetDirectoryName(path);
            if (parentDirectory == null)
            {
                throw new DirectoryNotFoundException($"No parent directory for '{path}'");
            }

            return parentDirectory;
        }

        public static string GetName(string path)
        {
            return Path.GetFileName(path);
        }

        public static FileType? GetFileType(string path)
        {
            if (File.Exists(path))
            {
                string extension = Path.GetExtension(path);
                switch (extension)
                {
                    case ".sln":
                        return FileType.SolutionFile;
                    case ".csproj":
                    case ".vbproj":
                    case ".fsproj": // TODO: Determine if F# is actually supported . . .
                        return FileType.ProjectFile;
                    default:
                        return FileType.Unknown;
                }
            }
            else
            {
                return null;
            }
        }

        public static string NormalizePath(string path)
        {
            return path.Replace("\\", "/");
        }

        public static HashSet<string> GetFilesFromProjectFile(string projectFilePath, string projectDirectory)
        {
            var files = new HashSet<string>();

            XDocument document = XDocument.Load(projectFilePath, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
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
                            files.Add($"{projectDirectory}/{includePath.Replace("\\", "/")}");
                        }
                    }
                }
            }

            return files;
        }

        public static HashSet<string> GetAllFilesFromGit(string repoPath) => GitHelper.GetAllFilesFromGit(repoPath);

        public static HashSet<string> GetFilesFromGitForProject(string repoPath, string projectDirectoryName) => GitHelper.GetFilesFromGitForProject(repoPath, projectDirectoryName);

        public static HashSet<string> GetFilesOnDisk(string directoryPath)
        {
            throw new NotImplementedException();
        }
    }
}
