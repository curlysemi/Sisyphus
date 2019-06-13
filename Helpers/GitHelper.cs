using LibGit2Sharp;
using Sisyphus.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sisyphus.Helpers
{
    public static class GitHelper
    {
        public static string GetRepoPathFromPath(string path)
        {
            IOExtensions.Vt(Program.IsVerbose, new { path });
            if (File.Exists(path))
            {
                if (!path.StartsWith(".\\") && $".\\{path}" is string fixedPath)
                {
                    return GetRepoPathFromFilePath(fixedPath);
                }
                else
                {
                    return GetRepoPathFromFilePath(path);
                }
            }
            else if (Directory.Exists(path))
            {
                return GetRepoPathFromDirectoryPath(path);
            }
            else
            {
                if (Path.HasExtension(path))
                {
                    throw new FileNotFoundException(path);
                }
                else
                {
                    throw new DirectoryNotFoundException(path);
                }
            }
        }

        public static string GetRepoPathFromFilePath(string filePath)
        {
            var parentDirectory = Path.GetDirectoryName(filePath);
            if (parentDirectory == null)
            {
                throw new DirectoryNotFoundException(Constants.GIT_DIR);
            }
            return GetRepoPathFromDirectoryPath(parentDirectory);
        }

        public static string GetRepoPathFromDirectoryPath(string directoryPath)
        {
            var directories = Directory.GetDirectories(directoryPath);
            if (directories.Any(d => Path.GetFileName(d) == Constants.GIT_DIR)) {
                return directoryPath;
            }
            else
            {
                return GetRepoPathFromFilePath(directoryPath);
            }
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

        public static HashSet<string> GetFilesFromGitForProject(string repoPath, string projectPath)
        {
            var absoluteProjectFileParentDirPath = FileHelper.GetParentDirectory(projectPath);
            var projectFileParentDirectoryName = FileHelper.GetName(absoluteProjectFileParentDirPath);
            var relativeProjectFileParentDir = Path.GetRelativePath(repoPath, absoluteProjectFileParentDirPath);

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

                var relevantTree = repo.Head.Tip.Tree.FirstOrDefault(t => t.Name == relativeProjectFileParentDir)?.Target;

                if (relevantTree is Tree projectTree)
                {
                    walkTreeAndFindFiles(projectTree);
                }
            }

            return files;
        }
    }
}
