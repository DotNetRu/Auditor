using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using DotNetRu.Auditor.Data;
using DotNetRu.Auditor.Storage.Collections.Changes;

using IdentityMap = System.Object;

namespace DotNetRu.Auditor.Storage.Collections
{
    // TDO: Add integration tests
    internal sealed class Session : ISession
    {
        public delegate bool CollectionResolver(Type collectionType, [NotNullWhen(true)] out Collection? collection);

        private readonly SessionOptions options;
        private readonly CollectionResolver tryResolveCollection;
        private readonly IDocumentSerializerFactory serializerFactory;
        private readonly ConcurrentDictionary<Type, IdentityMap> registry = new();

        public Session(
            SessionOptions options,
            CollectionResolver tryResolveCollection,
            IDocumentSerializerFactory serializerFactory)
        {
            this.options = options;
            this.tryResolveCollection = tryResolveCollection;
            this.serializerFactory = serializerFactory;
        }

        public async Task<T?> LoadAsync<T>(string id)
            where T : IDocument
        {
            var map = GetIdentityMap<T>();
            if (map.TryResolveDocument(id, out var cachedDocument))
            {
                return cachedDocument;
            }

            if (!tryResolveCollection(typeof(T), out var collection))
            {
                return default;
            }

            var serializer = serializerFactory.Create<T>();

            var document = await collection.LoadAsync(id, serializer.DeserializeAsync);

            if (document?.Id != null)
            {
                var registeredDocument = map.RegisterDocument(document);
                return registeredDocument;
            }

            return default;
        }

        public async Task<IReadOnlyDictionary<string, T>> LoadAsync<T>(IReadOnlyList<string> ids)
            where T : IDocument
        {
            var map = GetIdentityMap<T>();
            var cachedDocuments = map.GetDocuments(ids);
            if (cachedDocuments.Count == ids.Count)
            {
                return cachedDocuments;
            }

            if (!tryResolveCollection(typeof(T), out var collection))
            {
                return Dictionary.Empty<string, T>();
            }

            var serializer = serializerFactory.Create<T>();
            var documents = new Dictionary<string, T>(cachedDocuments);

            foreach (var id in ids.Except(documents.Keys))
            {
                var document = await collection.LoadAsync(id, serializer.DeserializeAsync).ConfigureAwait(false);

                if (document?.Id != null)
                {
                    var registeredDocument = map.RegisterDocument(document);
                    if (registeredDocument.Id != null)
                    {
                        documents.Add(registeredDocument.Id, registeredDocument);
                    }
                }
            }

            return documents;
        }

        public async IAsyncEnumerable<T> QueryAsync<T>()
            where T : IDocument
        {
            if (!tryResolveCollection(typeof(T), out var collection))
            {
                yield break;
            }

            // TODO: Perf: Check all document IDs in the map before query
            var map = GetIdentityMap<T>();

            var serializer = serializerFactory.Create<T>();

            await foreach (var document in collection.QueryAsync(serializer.DeserializeAsync).ConfigureAwait(false))
            {
                if (document?.Id != null)
                {
                    var registeredDocument = map.RegisterDocument(document);
                    yield return registeredDocument;
                }
            }
        }

        private IdentityMap<T> GetIdentityMap<T>()
            where T : IDocument =>
            (IdentityMap<T>)registry.GetOrAdd(typeof(T), _ => new IdentityMap<T>());
    }
}
