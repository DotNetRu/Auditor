using System;

namespace DotNetRu.Auditor.Storage
{
    public sealed class StoreOptions
    {
        public Func<Type, string> MapCollectionName { get; set; } = LowerPlural;

        private static string LowerPlural(Type type)
        {
            var typeName = type.Name.ToLowerInvariant();

            if (typeName.EndsWith("y", StringComparison.Ordinal))
            {
                typeName = typeName[..^1] + "ies";
            }
            else
            {
                typeName += "s";
            }

            return typeName;
        }
    }
}
