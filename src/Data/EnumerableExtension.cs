using System.Collections.Generic;
using System.Linq;

namespace DotNetRu.Auditor.Data
{
    internal static class EnumerableExtension
    {
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
            where T : class
        {
            // HACK: Current Roslyn analyzer can't recognize non-null elements without hacks
            return source.Where(x => x != null)!;
        }
    }
}
