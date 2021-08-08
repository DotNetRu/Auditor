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
        public delegate bool CollectionResolver(Type collectionType, [NotNullWhen(true)] out IDocumentCollection? collection);

        private readonly SessionOptions options;
        private readonly CollectionResolver tryResolveCollection;
        private readonly ConcurrentDictionary<Type, IdentityMap> identityMap = new();

        public Session(
            SessionOptions options,
            CollectionResolver tryResolveCollection)
        {
            this.options = options;
            this.tryResolveCollection = tryResolveCollection;
        }

        public async Task<T?> LoadAsync<T>(string id)
            where T : IDocument
        {
            // TDO: Move Identity map to collection wrapper
            var map = GetIdentityMap<T>();
            if (map.TryResolveDocument(id, out var cachedDocument))
            {
                return cachedDocument;
            }

            if (!tryResolveCollection(typeof(T), out var collectionBase))
            {
                return default;
            }

            var collection = (Collection<T>)collectionBase;

            var document = await collection.LoadAsync(id);

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

            if (!tryResolveCollection(typeof(T), out var collectionBase))
            {
                return Dictionary.Empty<string, T>();
            }

            var collection = (Collection<T>)collectionBase;
            var documents = new Dictionary<string, T>(cachedDocuments);

            foreach (var id in ids.Except(documents.Keys))
            {
                var document = await collection.LoadAsync(id).ConfigureAwait(false);

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
            if (!tryResolveCollection(typeof(T), out var collectionBase))
            {
                yield break;
            }

            // TODO: Perf: Check all document IDs in the map before query
            var map = GetIdentityMap<T>();
            var collection = (Collection<T>)collectionBase;

            await foreach (var document in collection.QueryAsync().ConfigureAwait(false))
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
            (IdentityMap<T>)identityMap.GetOrAdd(typeof(T), _ => new IdentityMap<T>());
    }
}
