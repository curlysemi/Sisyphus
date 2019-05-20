using CommandLine;
using Sisyphus.Commands.Base;
using Sisyphus.Core;
using Sisyphus.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using static Sisyphus.Helpers.IOExtensions;

namespace Sisyphus.Commands
{
    [Verb("verdep", HelpText = "Check the provided solution or project file for dependency conflicts.")]
    internal class VerDep : ProjectFileOrSolutionFileCommand
    {
        bool HasElement(XElement e, string elementName)
        {
            return IsElement(e.Descendants()?.FirstOrDefault(), elementName);
        }

        bool IsElement(XElement e, string elementName)
        {
            return e?.Name?.LocalName == elementName;
        }

        bool HasReference(XElement e)
        {
            return HasElement(e, "Reference");
        }

        bool IsReference(XElement e)
        {
            return IsElement(e, "Reference");
        }

        bool IsHintPath(XElement e)
        {
            return IsElement(e, "HintPath");
        }

        protected override (bool isSuccess, SError error) HandleProject(Config config, string repoPath, string projectPath)
        {
            // TODO: setting to warn of duplicates when treating git files in a case-insensitive manner . . .
            // Because `thing.txt` and `Thing.txt` could theoretically both exist . . .

            //var filesTrackedByGit = GitHelper.GetFilesFromGitForProject(repoPath, projectPath);
            //var filesIncludedInProjectFile = ProjectFileHelper.GetFilesFromProjectFile(projectPath, out string projectFileParentDirectoryName);

            //// Filter out project files, because project files do not include themselves . . .
            //var self = FileHelper.NormalizePath(Path.GetRelativePath(repoPath, projectPath));
            //filesTrackedByGit.Remove(self);

            //// Remove any other files our config says we can ignore before we compare . . .
            //filesTrackedByGit.RemoveWhere(config.IsIgnorable);

            //// Project files are case-insensitive . . .
            //// But git is case-sensitive . . .
            //var filesNotIncludedInProjectFile = filesTrackedByGit.Where(m => !filesIncludedInProjectFile.Contains(m, StringComparer.CurrentCultureIgnoreCase)).ToList();

            //if (filesNotIncludedInProjectFile?.Any() == true)
            //{
            //    Log(projectFileParentDirectoryName + ":");
            //    foreach (var file in filesNotIncludedInProjectFile)
            //    {
            //        LogError($" ({Ordinal}) \t{file}");
            //        Ordinal++;
            //    }
            //    NL();
            //}

            var packageRefs = GetPackageReferencesFromProjectFile(projectPath);
            var packagesConf = GetPackagesFromPackagesDotConfig(projectPath);

            // TODO: Determine if projects can have multiple references to the same package (but different versions)?

            return Success;
        }

        public List<PackageReference> GetPackageReferencesFromProjectFile(string projectPath)
        {
            var packageRefs = new List<PackageReference>();

            var (document, itemGroups) = ProjectFileHelper.LoadProjectXml(projectPath);

            var refItemGroup = itemGroups.FirstOrDefault(HasReference);
            var refsOnly = refItemGroup.Descendants()?.Where(IsReference);

            foreach (var @ref in refsOnly)
            {
                string hintPath = @ref.Descendants()?.FirstOrDefault(IsHintPath)?.Value;

                string packageIncludeString = @ref.Attributes().FirstOrDefault(a => a.Name == "Include")?.Value;
                if (packageIncludeString != null)
                {
                    var packageRef = new PackageReference(packageIncludeString, hintPath);
                    packageRefs.Add(packageRef);
                }
            }

            return packageRefs;
        }

        public List<string> GetPackagesFromPackagesDotConfig(string projectPath)
        {
            var packages = new List<string>();

            var projectDir = FileHelper.GetParentDirectory(projectPath);
            var packageJsonFilePath = Path.Join(projectDir, "packages.config");
            XDocument document = XDocument.Load(packageJsonFilePath, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);

            //document.Root

            return packages;
        }
    }
}
