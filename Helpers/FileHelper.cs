using System;
using System.Collections.Generic;
using System.Text;
using LibGit2Sharp;
using System.Linq;
using System.Xml.Linq;

namespace Sisyphus.Helpers
{
    public static class FileHelper
    {
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

        public static HashSet<string> GetAllFilesFromGit(string repoPath)
        {
            var files = new HashSet<string>();
            using (var repo = new Repository(repoPath))
            {
                foreach (IndexEntry e in repo.Index)
                {
                    if (e.StageLevel == StageLevel.Staged)
                    {
                        files.Add(e.Path);
                    }
                }
            }

            return files;
        }

        #region Naive GetFilesFromGitForProject
        /*
        public static HashSet<string> GetFilesFromGitForProject(string repoPath, string projectDirectoryName)
        {
            var files = new HashSet<string>();
            using (var repo = new Repository(repoPath))
            {
                bool found = false;

                foreach (IndexEntry e in repo.Index)
                {
                    if (e.StageLevel == StageLevel.Staged)
                    {
                        if (e.Path.StartsWith(projectDirectoryName))
                        {
                            files.Add(e.Path);
                            found = true;
                        }
                        else if (found)
                        {
                            // We don't need to keep iterating, since we're out of the project directory
                            break;
                        }
                    }
                }
            }

            return files;
        }
        */
        #endregion

        public static HashSet<string> GetFilesFromGitForProject(string repoPath, string projectDirectoryName)
        {
            var files = new HashSet<string>();

            using (var repo = new Repository(repoPath))
            {
                void walkTreeAndFindFiles(Tree t)
                {
                    foreach (var item in t)
                    {
                        switch (item.TargetType)
                        {
                            case TreeEntryTargetType.Blob:
                                files.Add(item.Path);
                                break;
                            case TreeEntryTargetType.Tree:
                                walkTreeAndFindFiles(item.Target as Tree);
                                break;
                            default:
                                throw new NotImplementedException($"The following case was not handled: {item.TargetType.ToString()}");
                        }
                    }
                }

                var relevantTree = repo.Head.Tip.Tree.FirstOrDefault(t => t.Name == projectDirectoryName)?.Target;

                if (relevantTree is Tree projectTree)
                {
                    walkTreeAndFindFiles(projectTree);
                }
            }

            return files;
        }

        public static HashSet<string> GetFilesOnDisk(string directoryPath)
        {
            throw new NotImplementedException();
        }
    }
}
