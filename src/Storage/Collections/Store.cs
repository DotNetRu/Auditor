using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DotNetRu.Auditor.Storage.Collections
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

        public ISession OpenSession(SessionOptions? sessionOptions = null)
        {
            sessionOptions ??= new SessionOptions();
            return new Session(sessionOptions, TryGetCollection);
        }

        private bool TryGetCollection(Type collectionType, [NotNullWhen(true)] out IDocumentCollection? collection)
        {
            return collections.TryGetValue(collectionType, out collection);
        }
    }
}
