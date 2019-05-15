using CommandLine;
using Sisyphus.Commands.Base;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Sisyphus.Commands
{
    [Verb("dedup", HelpText = "Remove duplicate file-references from project file")]
    class Dedup : ProjectFileCommand
    {
        protected override bool ActOnProject(ref XElement[] itemGroups)
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