// Copyright 2016 Kirill Osenkov
// https://github.com/KirillOsenkov/CodeCleanupTools/blob/master/RemoveDuplicateItems/RemoveDuplicateItems.cs

using CommandLine;
using Sisyphus.Commands.Base;
using Sisyphus.Core;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Sisyphus.Commands
{
    [Verb("dedup", HelpText = "Remove duplicate file-references from project file or projects in solution file. Sorting first is recommended.")]
    internal class Dedup : PortedProjectFileOrSolutionFileCommand
    {
        protected override bool ActOnProject(Config config, ref XElement[] itemGroups)
        {
            foreach (XElement itemGroup in itemGroups)
            {
                var original = itemGroup.Elements().ToArray();
                var visited = new HashSet<string>();
                foreach (var item in original)
                {
                    // if we've seen this node before, remove it
                    if (!visited.Add(item.ToString(SaveOptions.DisableFormatting)))
                    {
                        if (item.PreviousNode is XText previousTrivia)
                        {
                            previousTrivia.Remove();
                        }

                        item.Remove();
                    }
                }
            }

            return true;
        }
    }
}