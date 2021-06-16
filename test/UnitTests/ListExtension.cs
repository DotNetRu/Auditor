using System;
using System.Collections.Generic;

namespace DotNetRu.Auditor.UnitTests
{
    internal static class ListExtension
    {
        public static int GetItemsHashCode<T>(this IEnumerable<T> list)
        {
            var hash = new HashCode();

            foreach (var item in list)
            {
                hash.Add(item);
            }

            return hash.ToHashCode();
        }
    }
}
