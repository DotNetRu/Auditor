using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DotNetRu.Auditor.Data;
using DotNetRu.Auditor.Data.Xml;

namespace DotNetRu.Auditor.Storage.Collections
{
    // TDO: Add integration tests
    internal sealed class Store : IStore
    {
        private readonly IDataSerializerFactory serializerFactory = new XmlDataSerializerFactory();
        private readonly StoreOptions options;
        private readonly IReadOnlyDictionary<string, Collection> collections;

        public Store(StoreOptions options, IReadOnlyList<Collection> collections)
        {
            this.options = options;
            this.collections = collections.ToDictionary(collection => collection.Name);
        }

        public ISession OpenSession(SessionOptions? sessionOptions = null)
        {
            sessionOptions ??= new SessionOptions();
            return new Session(sessionOptions, TryGetCollection, serializerFactory);
        }

        private bool TryGetCollection(Type collectionType, [NotNullWhen(true)] out Collection? collection)
        {
            var collectionName = options.MapCollectionName(collectionType);
            return collections.TryGetValue(collectionName, out collection);
        }
    }
}
