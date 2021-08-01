using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using DotNetRu.Auditor.Data;

namespace DotNetRu.Auditor.Storage.Collections
{
    internal sealed class Session : ISession
    {
        public delegate bool CollectionResolver(Type collectionType, [NotNullWhen(true)] out Collection? collection);

        private readonly SessionOptions options;
        private readonly CollectionResolver tryResolveCollection;
        private readonly IDocumentSerializerFactory serializerFactory;

        public Session(
            SessionOptions options,
            CollectionResolver tryResolveCollection,
            IDocumentSerializerFactory serializerFactory)
        {
            this.options = options;
            this.tryResolveCollection = tryResolveCollection;
            this.serializerFactory = serializerFactory;
        }

        public Task<T?> LoadAsync<T>(string id)
            where T : IDocument
        {
            if (!tryResolveCollection(typeof(T), out var collection))
            {
                return Task.FromResult<T?>(default);
            }

            var serializer = serializerFactory.Create<T>();
            return collection.LoadAsync(id, serializer.DeserializeAsync);
        }

        public async Task<IReadOnlyDictionary<string, T>> LoadAsync<T>(IReadOnlyList<string> ids)
            where T : IDocument
        {
            if (!tryResolveCollection(typeof(T), out var collection))
            {
                return Dictionary.Empty<string, T>();
            }

            var serializer = serializerFactory.Create<T>();
            var documents = new Dictionary<string, T>(ids.Count);

            foreach (var id in ids)
            {
                var document = await collection.LoadAsync(id, serializer.DeserializeAsync).ConfigureAwait(false);
                if (document != null && document.Id != null)
                {
                    documents.Add(document.Id, document);
                }
            }

            return documents;
        }

        public IAsyncEnumerable<T> QueryAsync<T>()
            where T : IDocument
        {
            if (!tryResolveCollection(typeof(T), out var collection))
            {
                return AsyncEnumerable.Empty<T>();
            }

            var serializer = serializerFactory.Create<T>();
            var documents = collection.QueryAsync(serializer.DeserializeAsync);
            return documents;
        }
    }
}
