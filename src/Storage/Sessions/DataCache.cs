using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DotNetRu.Auditor.Data;

namespace DotNetRu.Auditor.Storage.Sessions
{
    internal sealed class DataCache : IDataCache
    {
        private readonly ConcurrentDictionary<Type, IIdentityMap> identityMaps = new();

        public IdentityMap<T> GetMap<T>()
            where T : IDocument =>
            (IdentityMap<T>)identityMaps.GetOrAdd(typeof(T), _ => new IdentityMap<T>());

        public IReadOnlyList<IIdentityMap> GetAllMaps() => identityMaps.Values.ToList();
    }
}
