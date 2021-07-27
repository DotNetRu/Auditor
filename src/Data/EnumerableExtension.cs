﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetRu.Auditor.Data
{
    public static class EnumerableExtension
    {
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
            where T : class
        {
            // HACK: Current Roslyn analyzer can't recognize non-null elements without hacks
            return source.Where(x => x != null)!;
        }

        public static Task WhenAll(this IEnumerable<Task> tasks) => Task.WhenAll(tasks);
    }
}
