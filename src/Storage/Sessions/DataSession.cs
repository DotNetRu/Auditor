using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using DotNetRu.Auditor.Data;
using DotNetRu.Auditor.Storage.Collections;

namespace DotNetRu.Auditor.Storage.Sessions
{
    internal interface IDataSession : ISession
    {
        Task WriteAsync<T>(IReadOnlyList<T> documents)
            where T : IDocument;
    }

    internal sealed class DataSession : IDataSession
    {
        public delegate bool CollectionResolver(Type collectionType, [NotNullWhen(true)] out IDocumentCollection? collection);
        private readonly CollectionResolver tryResolveCollection;

        public DataSession(CollectionResolver tryResolveCollection)
        {
            this.tryResolveCollection = tryResolveCollection;
        }

        public Task<T?> LoadAsync<T>(string id)
            where T : IDocument
        {
            if (!TryResolveCollection<T>(out var collection))
            {
                return Task.FromResult<T?>(default);
            }

            return collection.LoadAsync(id);
        }

        public async Task<IReadOnlyDictionary<string, T>> LoadAsync<T>(IReadOnlyList<string> ids)
            where T : IDocument
        {
            if (!TryResolveCollection<T>(out var collection))
            {
                return Dictionary.Empty<string, T>();
            }

            var documents = new Dictionary<string, T>();

            foreach (var id in ids)
            {
                var document = await collection.LoadAsync(id).ConfigureAwait(false);

                if (document?.Id != null)
                {
                    documents.Add(document.Id, document);
                }
            }

            return documents;
        }

        public IAsyncEnumerable<T> QueryAsync<T>()
            where T : IDocument
        {
            if (!TryResolveCollection<T>(out var collection))
            {
                return AsyncEnumerable.Empty<T>();
            }

            return collection.QueryAsync();
        }

        public Task AddAsync<T>(T document)
            where T : IDocument
        {
            return WriteAsync(document.AsEnumerable());
        }

        public Task WriteAsync<T>(IReadOnlyList<T> documents)
            where T : IDocument
        {
            if (!TryResolveCollection<T>(out var collection))
            {
                throw new InvalidOperationException($"Collection «{typeof(T)}» not found");
            }

            return documents
                .Select(collection.WriteAsync)
                .WhenAll();
        }

        private bool TryResolveCollection<T>([NotNullWhen(true)] out Collection<T>? collection)
            where T : IDocument
        {
            if (!tryResolveCollection(typeof(T), out var collectionBase))
            {
                collection = default;
                return false;
            }

            collection = (Collection<T>)collectionBase;
            return true;
        }
    }
}
