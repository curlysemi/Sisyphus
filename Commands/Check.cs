using CommandLine;
using Sisyphus.Commands.Base;
using Sisyphus.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sisyphus.Commands
{
    [Verb("check", HelpText = "Check the provided solution or project file for issues (defaults to whatever is in current directory).")]
    public class Check : BaseCommand
    {
        [Option('i', "input", HelpText = "Input git repository path")]
        public string RepoPath { get; set; }

        public override (bool isSuccess, SError error) Run()
        {
            Console.WriteLine("Hello World!");

            // TODO: Actually implement

            // We'll just list files . . .
            var misc1 = Helpers.FileHelper.GetFilesFromGitForProject(RepoPath, "Misc");

            var projDocs = Helpers.FileHelper.GetFilesFromProjectFile(RepoPath + "\\Misc\\Misc.csproj", "Misc");

            var filesNotIncludedInProjectFile = misc1.Where(m => !projDocs.Contains(m)).ToList();

            //misc1.Contains()

            return Success;
        }
    }
}
