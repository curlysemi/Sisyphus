using System;
using System.Collections.Generic;
using System.Text;

namespace Sisyphus.Helpers
{
    internal static class Extensions
    {
        public static void AddRange<T>(this HashSet<T> set, params T[] elements)
        {
            foreach (var element in elements)
            {
                set.Add(element);
            }
        }
    }
}
