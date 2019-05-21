﻿using CommandLine;
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
        [Option('p', "hint-paths", HelpText = "Print protential HintPath discrepancies")]
        public bool ShouldPrintPotentialHintPathDiscrepancies { get; set; }

        [Option('f', "ignore-framework", HelpText = "When checking HintPath discrepancies, ignore differences in target frameworks")]
        public bool ShouldIgnoreTargetFrameworks { get; set; }

        [Option('d', "on-disk", HelpText = "When checking HintPath discrepancies, check that the packages are on disk")]
        public bool ShouldCheckPackagesOnDisk { get; set; }

        private int NumDiscrepancies { get; set; } = 0;
        private int NumNoHPs { get; set; } = 0;
        private int NumFine { get; set; } = 0;

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

        private string MakeAgnosticHintPath(string hintPath, string depName)
        {
            const string lib = @"\lib\";
            var libIndex = hintPath.LastIndexOf(lib);
            var dllName = $"{depName}.dll";
            var dllIndex = hintPath.LastIndexOf(dllName);

            var agnosticPath = hintPath.Substring(0, libIndex + lib.Length) + hintPath.Substring(dllIndex, dllName.Length);

            return agnosticPath;
        }

        protected override (bool isSuccess, SError error) HandleProject(Config config, string repoPath, string projectPath)
        {
            var projName = ProjectFileHelper.GetProjectFileParentDirName(projectPath);

            var packageRefs = GetPackageReferencesFromProjectFile(projectPath);
            var packagesConf = GetPackagesFromPackagesDotConfig(projectPath);

            var primaryDependencies = packageRefs.Where(p => packagesConf.Any(c => c.Name == p.Name));

            if (ShouldPrintPotentialHintPathDiscrepancies)
            {
                foreach (var primaryDep in primaryDependencies)
                {
                    var relevantConf = packagesConf.First(c => c.Name == primaryDep.Name);
                    var projectedHintPath = relevantConf.ProjectHintPath(config);
                    var hintPath = primaryDep.HintPath?.Path;
                    bool hasHintPath = hintPath != null;
                    bool isFine = true;
                    if (hasHintPath && !string.Equals(hintPath, projectedHintPath, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (ShouldIgnoreTargetFrameworks)
                        {
                            var agnosticHintPath = MakeAgnosticHintPath(hintPath, primaryDep.Name);
                            var agnosticProjectedPath = MakeAgnosticHintPath(projectedHintPath, relevantConf.Name);

                            if (string.Equals(agnosticHintPath, agnosticProjectedPath, StringComparison.InvariantCultureIgnoreCase))
                            {
                                goto isFineHandler;
                            }
                        }
                        isFine = false;
                        NumDiscrepancies++;
                        Log($"{projName}'s '{primaryDep.Name}':");
                        Log("HP:\t" + hintPath);
                        Log("GP:\t" + projectedHintPath);
                        NL();
                    }
                    else if (!hasHintPath)
                    {
                        isFine = false;
                        Log($"{projName}'s '{primaryDep.Name}' has no hint path");
                        NL();
                        NumNoHPs++;
                    }

                    isFineHandler:
                    if (isFine)
                    {
                        NumFine++;
                    }
                }
            }

            // TODO: Determine if projects can have multiple references to the same package (but different versions)?

            return Success;
        }

        protected override void AfterAll(Config config, string repoPath, ref List<string> absoluteProjectFilePaths)
        {
            Log($"Number of discrepancies:  {NumDiscrepancies}");
            Log($"Number of no HintPaths:   {NumNoHPs}");
            Log($"Number of fine HintPaths: {NumFine}");
        }

        public List<DepReference> GetPackageReferencesFromProjectFile(string projectPath)
        {
            var packageRefs = new List<DepReference>();

            var (document, itemGroups) = ProjectFileHelper.LoadProjectXml(projectPath);

            var refItemGroup = itemGroups.FirstOrDefault(HasReference);
            var refsOnly = refItemGroup?.Descendants()?.Where(IsReference);

            if (refsOnly?.Any() == true)
            {
                foreach (var @ref in refsOnly)
                {
                    string hintPath = @ref.Descendants()?.FirstOrDefault(IsHintPath)?.Value;

                    string packageIncludeString = @ref.Attributes().FirstOrDefault(a => a.Name == "Include")?.Value;
                    if (packageIncludeString != null)
                    {
                        var packageRef = new DepReference(packageIncludeString, hintPath);
                        packageRefs.Add(packageRef);
                    }
                }
            }

            return packageRefs;
        }

        public List<DepConfig> GetPackagesFromPackagesDotConfig(string projectPath)
        {
            var packages = new List<DepConfig>();

            var projectDir = FileHelper.GetParentDirectory(projectPath);
            var packageJsonFilePath = Path.Join(projectDir, "packages.config");
            XDocument document = XDocument.Load(packageJsonFilePath, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);

            var packageElements = document.Root.Elements("package");
            if (packageElements?.Any() == true)
            {
                foreach (var package in packageElements)
                {
                    var depConfig = new DepConfig(package);
                    packages.Add(depConfig);
                }
            }

            return packages;
        }
    }
}
