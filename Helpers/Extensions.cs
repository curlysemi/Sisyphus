using System.Collections.Generic;

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
