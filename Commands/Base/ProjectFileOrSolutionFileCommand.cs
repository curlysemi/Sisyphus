using CommandLine;
using Sisyphus.Core;
using Sisyphus.Core.Enums;
using Sisyphus.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Sisyphus.Commands.Base
{
    internal abstract class ProjectFileOrSolutionFileCommand : BaseCommand
    {
        [Option('i', "input", HelpText = "Input project file of solution file path")]
        public string ProjectFileOrSolutionFilePath { get; set; }

        protected virtual void BeforeAll(string repoPath, ref List<string> absoluteProjectFilePaths) { }

        protected abstract (bool isSuccess, SError error) HandleProject(string repoPath, string projectPath);

        protected virtual void AfterAll(string repoPath, ref List<string> absoluteProjectFilePaths) { }

        protected override (bool isSuccess, SError error) Run(Config config)
        {
            var absoluteProjectFilePaths = new List<string>();

            var fileType = FileHelper.GetFileType(ProjectFileOrSolutionFilePath);
            switch (fileType)
            {
                case FileType.ProjectFile:
                    {
                        absoluteProjectFilePaths.Add(ProjectFileOrSolutionFilePath);
                        break;
                    }
                case FileType.SolutionFile:
                    {
                        var absoluteProjectFilePathsFromSolution = SolutionFileHelper.GetAbsoluteProjectPaths(ProjectFileOrSolutionFilePath);
                        absoluteProjectFilePaths.AddRange(absoluteProjectFilePathsFromSolution.Where(p => FileHelper.GetFileType(p) == FileType.ProjectFile));
                        break;
                    }
                case null:
                    {
                        return Error($"'{ProjectFileOrSolutionFilePath}' is not a file!");
                    }
                case FileType.Unknown:
                default:
                    {
                        return Error($"'{ProjectFileOrSolutionFilePath}' is not a known project file type or solution file!");
                    }
            }

            var repoPath = GitHelper.GetRepoPathFromPath(ProjectFileOrSolutionFilePath);

            BeforeAll(repoPath, ref absoluteProjectFilePaths);

            foreach (var projectPath in absoluteProjectFilePaths)
            {
                var (isProjectSuccess, projectError) = HandleProject(repoPath, projectPath);
                if (!isProjectSuccess)
                {
                    return Error(projectError);
                }
            }

            AfterAll(repoPath, ref absoluteProjectFilePaths);

            return Success;
        }
    }
}
