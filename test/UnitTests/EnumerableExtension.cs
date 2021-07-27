using System;
using System.Collections.Generic;

namespace DotNetRu.Auditor.UnitTests
{
    internal static class EnumerableExtension
    {
        public static int GetItemsHashCode<T>(this IEnumerable<T> items)
        {
            var hash = new HashCode();

            foreach (var item in items)
            {
                hash.Add(item);
            }

            return hash.ToHashCode();
        }
    }
}
