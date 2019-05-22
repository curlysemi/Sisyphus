using CommandLine;
using Sisyphus.Commands.Base;
using Sisyphus.Core;
using Sisyphus.Helpers;
using System;
using System.IO;
using System.Linq;

using static Sisyphus.Helpers.IOExtensions;

namespace Sisyphus.Commands
{
    [Verb("check", HelpText = "Check the provided solution or project file for missing includes.")]
    internal class Check : ProjectFileOrSolutionFileCommand
    {
        private int Ordinal { get; set; } = 1;

        protected override (bool isSuccess, SError error) HandleProject(Config config, string repoPath, string projectPath)
        {
            // TODO: setting to warn of duplicates when treating git files in a case-insensitive manner . . .
            // Because `thing.txt` and `Thing.txt` could theoretically both exist . . .

            var filesTrackedByGit = GitHelper.GetFilesFromGitForProject(repoPath, projectPath);
            var filesIncludedInProjectFile = ProjectFileHelper.GetFilesFromProjectFile(projectPath, out string projectFileParentDirectoryName);

            // Filter out project files, because project files do not include themselves . . .
            var self = FileHelper.NormalizePath(Path.GetRelativePath(repoPath, projectPath));
            filesTrackedByGit.Remove(self);

            // Remove any other files our config says we can ignore before we compare . . .
            filesTrackedByGit.RemoveWhere(config.IsIgnorable);

            // Project files are case-insensitive . . .
            // But git is case-sensitive . . .
            var filesNotIncludedInProjectFile = filesTrackedByGit.Where(m => !filesIncludedInProjectFile.Contains(m, StringComparer.CurrentCultureIgnoreCase)).ToList();

            if (filesNotIncludedInProjectFile?.Any() == true)
            {
                Log(projectFileParentDirectoryName + ":");
                foreach (var file in filesNotIncludedInProjectFile)
                {
                    LogError($" ({Ordinal}) \t{file}");
                    Ordinal++;
                }
                NL();
            }

            return Success;
        }
    }
}
