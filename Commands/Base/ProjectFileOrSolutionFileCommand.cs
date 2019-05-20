﻿using CommandLine;
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

        protected virtual void BeforeAll(Config config, string repoPath, ref List<string> absoluteProjectFilePaths) { }

        protected abstract (bool isSuccess, SError error) HandleProject(Config config, string repoPath, string projectPath);

        protected virtual void AfterAll(Config config, string repoPath, ref List<string> absoluteProjectFilePaths) { }

        protected override (bool isSuccess, SError error) PreRunSetup(ref Config config)
        {
            var fileType = FileHelper.GetFileType(ProjectFileOrSolutionFilePath);
            switch (fileType)
            {
                case FileType.ProjectFile:
                case FileType.SolutionFile:
                    {
                        TargetFileType = fileType.Value;
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

            var absoluteProjectOrSolutionDirectoryPath = FileHelper.GetParentDirectory(ProjectFileOrSolutionFilePath);
            Config.TryLoadConfigFromPathIfNull(absoluteProjectOrSolutionDirectoryPath, ref config, suppressWarning: true);

            RepoPath = GitHelper.GetRepoPathFromPath(ProjectFileOrSolutionFilePath);

            if (RepoPath != absoluteProjectOrSolutionDirectoryPath)
            {
                Config.TryLoadConfigFromPathIfNull(RepoPath, ref config, suppressWarning: true);
            }

            return base.PreRunSetup(ref config);
        }

        private FileType TargetFileType { get; set; }

        private string RepoPath { get; set; }

        protected override (bool isSuccess, SError error) Run(Config config)
        {
            var absoluteProjectFilePaths = new List<string>();

            switch (TargetFileType)
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
                default:
                    {
                        return Error($"'{ProjectFileOrSolutionFilePath}' is not a known project file type or solution file!");
                    }
            }

            BeforeAll(config, RepoPath, ref absoluteProjectFilePaths);

            foreach (var projectPath in absoluteProjectFilePaths)
            {
                var (isProjectSuccess, projectError) = HandleProject(config, RepoPath, projectPath);
                if (!isProjectSuccess)
                {
                    return Error(projectError);
                }
            }

            AfterAll(config, RepoPath, ref absoluteProjectFilePaths);

            return Success;
        }
    }
}