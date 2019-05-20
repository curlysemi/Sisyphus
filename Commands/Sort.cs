// Copyright 2015 Kirill Osenkov
// https://github.com/KirillOsenkov/CodeCleanupTools/blob/master/SortProjectItems/SortProjectItems.cs

using CommandLine;
using Sisyphus.Commands.Base;
using Sisyphus.Core;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Sisyphus.Commands
{
    [Verb("sort", HelpText = "Sort the contents of the provided project file or projects in provided solution file.")]
    internal class Sort : PortedProjectFileOrSolutionFileCommand
    {
        protected override bool ActOnProject(Config config, ref XElement[] itemGroups)
        {
            var processedItemGroups = new List<XElement>();

            CombineCompatibleItemGroups(itemGroups, processedItemGroups);

            foreach (XElement itemGroup in processedItemGroups)
            {
                SortItemGroup(itemGroup);
            }

            return true;
        }

        private void CombineCompatibleItemGroups(XElement[] itemGroups, List<XElement> processedItemGroups)
        {
            var itemTypeLookup = itemGroups.ToDictionary(i => i, i => GetItemTypesFromItemGroup(i));
            foreach (var itemGroup in itemGroups)
            {
                if (!itemGroup.HasElements)
                {
                    RemoveItemGroup(itemGroup);
                    continue;
                }

                var suitableExistingItemGroup = FindSuitableItemGroup(processedItemGroups, itemGroup, itemTypeLookup);
                if (suitableExistingItemGroup != null)
                {
                    ReplantAllItems(from: itemGroup, to: suitableExistingItemGroup);

                    RemoveItemGroup(itemGroup);
                }
                else
                {
                    processedItemGroups.Add(itemGroup);
                }
            }
        }

        private void RemoveItemGroup(XElement itemGroup)
        {
            var leadingTrivia = itemGroup.PreviousNode;
            if (leadingTrivia is XText)
            {
                leadingTrivia.Remove();
            }

            itemGroup.Remove();
        }

        private void ReplantAllItems(XElement from, XElement to)
        {
            if (to.LastNode is XText)
            {
                to.LastNode.Remove();
            }

            var fromNodes = from.Nodes().ToArray();
            from.RemoveNodes();
            foreach (var element in fromNodes)
            {
                to.Add(element);
            }
        }

        private XElement FindSuitableItemGroup(
            List<XElement> existingItemGroups,
            XElement itemGroup,
            Dictionary<XElement, HashSet<string>> itemTypeLookup)
        {
            foreach (var existing in existingItemGroups)
            {
                var itemTypesInExisting = itemTypeLookup[existing];
                var itemTypesInCurrent = itemTypeLookup[itemGroup];
                if (itemTypesInCurrent.IsSubsetOf(itemTypesInExisting) && AreItemGroupsMergeable(itemGroup, existing))
                {
                    return existing;
                }
            }

            return null;
        }

        private bool AreItemGroupsMergeable(XElement left, XElement right)
        {
            if (!AttributeMissingOrSame(left, right, "Label"))
            {
                return false;
            }

            if (!AttributeMissingOrSame(left, right, "Condition"))
            {
                return false;
            }

            return true;
        }

        private bool AttributeMissingOrSame(XElement left, XElement right, string attributeName)
        {
            var leftAttribute = left.Attribute(attributeName);
            var rightAttribute = right.Attribute(attributeName);
            if (leftAttribute == null && rightAttribute == null)
            {
                return true;
            }
            else if (leftAttribute != null && rightAttribute != null)
            {
                return leftAttribute.Value == rightAttribute.Value;
            }

            return false;
        }

        private HashSet<string> GetItemTypesFromItemGroup(XElement itemGroup)
        {
            var set = new HashSet<string>();
            foreach (var item in itemGroup.Elements())
            {
                set.Add(item.Name.LocalName);
            }

            return set;
        }

        private void SortItemGroup(XElement itemGroup)
        {
            var original = itemGroup.Elements().ToArray();
            var sorted = original
                .OrderBy(i => i.Name.LocalName)
                .ThenBy(i => (i.Attribute("Include") ?? i.Attribute("Remove")).Value)
                .ToArray();

            for (int i = 0; i < original.Length; i++)
            {
                original[i].ReplaceWith(sorted[i]);
            }
        }
    }
}
