using CommandLine;
using Sisyphus.Commands.Base;
using Sisyphus.Core;
using Sisyphus.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sisyphus.Commands
{
    [Verb("check", HelpText = "Check the provided solution or project file for issues (defaults to whatever is in current directory).")]
    public class Check : ProjectFileOrSolutionFileCommand
    {
        private int Ordinal { get; set; } = 1;

        protected override (bool isSuccess, SError error) HandleProject(string repoPath, string projectPath)
        {
            // Project files are case-insensitive . . .
            // But git is case-sensitive . . .

            // TODO: setting to warn of duplicates when treating git files in a case-insensitive manner . . .
            // Because `thing.txt` and `Thing.txt` could theoretically both exist . . .

            var absoluteProjectFileParentDirPath = FileHelper.GetParentDirectory(projectPath);
            var projectFileParentDirectoryName = FileHelper.GetName(absoluteProjectFileParentDirPath);
            var relativeProjectFileParentDir = Path.GetRelativePath(repoPath, absoluteProjectFileParentDirPath);

            var filesTrackedByGit = FileHelper.GetFilesFromGitForProject(repoPath, relativeProjectFileParentDir);
            var filesIncludedInProjectFile = FileHelper.GetFilesFromProjectFile(projectPath, projectFileParentDirectoryName);

            // Filter out project files, because project files do not include themselves . . .
            var self = FileHelper.NormalizePath(Path.GetRelativePath(repoPath, projectPath));
            filesTrackedByGit.Remove(self);

            var filesNotIncludedInProjectFile = filesTrackedByGit.Where(m => !filesIncludedInProjectFile.Contains(m, StringComparer.CurrentCultureIgnoreCase)).ToList();

            if (filesNotIncludedInProjectFile?.Any() == true)
            {
                Log(projectFileParentDirectoryName + ":");
                foreach (var file in filesNotIncludedInProjectFile)
                {
                    LogError($" ({Ordinal}) \t{file}");
                    Ordinal++;
                }
                Log("\n");
            }

            return Success;
        }
    }
}
