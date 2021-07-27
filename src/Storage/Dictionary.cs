using System.Collections.Generic;

namespace DotNetRu.Auditor.Storage
{
    internal static class Dictionary
    {
        public static IReadOnlyDictionary<TKey, TValue> Empty<TKey, TValue>()
            where TKey : notnull
        {
            return EmptyDictionary<TKey, TValue>.Instance;
        }

        private static class EmptyDictionary<TKey, TValue>
            where TKey : notnull
        {
            public static readonly IReadOnlyDictionary<TKey, TValue> Instance = new Dictionary<TKey, TValue>(0);
        }
    }
}
