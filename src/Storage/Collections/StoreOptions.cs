﻿using System;

namespace DotNetRu.Auditor.Storage.Collections
{
    public sealed class StoreOptions
    {
        public Func<Type, string> MapCollectionName { get; set; } = LowerPlural;

        private static string LowerPlural(Type recordType)
        {
            var typeName = recordType.Name.Replace("Record", String.Empty).ToLowerInvariant();

            if (typeName.EndsWith("y", StringComparison.Ordinal))
            {
                typeName = typeName[0..^1] + "ies";
            }
            else
            {
                typeName += "s";
            }

            return typeName;
        }
    }
}
