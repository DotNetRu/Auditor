using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DotNetRu.Auditor.Storage.Collections;

namespace DotNetRu.Auditor.Storage.Sessions
{
    // TDO: Add integration tests
    internal sealed class Store : IStore
    {
        private readonly StoreOptions options;
        private readonly IReadOnlyDictionary<Type, IDocumentCollection> collections;

        public Store(StoreOptions options, IReadOnlyList<IDocumentCollection> collections)
        {
            this.options = options;
            this.collections = collections.ToDictionary(collection => collection.CollectionType);
        }

        public ISession OpenSession()
        {
            var dataSession = new DataSession(TryGetCollection);
            return new CacheableSession(dataSession);
        }

        public IReadOnlySession OpenReadOnlySession()
        {
            return OpenSession();
        }

        private bool TryGetCollection(Type collectionType, [NotNullWhen(true)] out IDocumentCollection? collection)
        {
            return collections.TryGetValue(collectionType, out collection);
        }
    }
}
