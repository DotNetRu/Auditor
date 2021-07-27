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
        private readonly IDataSerializerFactory serializerFactory;

        public Session(
            SessionOptions options,
            CollectionResolver tryResolveCollection,
            IDataSerializerFactory serializerFactory)
        {
            this.options = options;
            this.tryResolveCollection = tryResolveCollection;
            this.serializerFactory = serializerFactory;
        }

        public Task<T?> LoadAsync<T>(string id)
            where T : IRecord
        {
            if (!tryResolveCollection(typeof(T), out var collection))
            {
                return Task.FromResult<T?>(default);
            }

            var serializer = serializerFactory.Create<T>();
            var record = collection.LoadAsync(id, serializer.DeserializeAsync);
            return record;
        }

        public async Task<IReadOnlyDictionary<string, T>> LoadAsync<T>(IReadOnlyList<string> ids)
            where T : IRecord
        {
            if (!tryResolveCollection(typeof(T), out var collection))
            {
                return Dictionary.Empty<string, T>();
            }

            var serializer = serializerFactory.Create<T>();
            var records = new Dictionary<string, T>(ids.Count);

            foreach (var id in ids)
            {
                var record = await collection.LoadAsync(id, serializer.DeserializeAsync).ConfigureAwait(false);
                if (record != null && record.Id != null)
                {
                    records.Add(record.Id, record);
                }
            }

            return records;
        }

        public IAsyncEnumerable<T> QueryAsync<T>()
            where T : IRecord
        {
            if (!tryResolveCollection(typeof(T), out var collection))
            {
                return AsyncEnumerable.Empty<T>();
            }

            var serializer = serializerFactory.Create<T>();
            var records = collection.QueryAsync(serializer.DeserializeAsync);
            return records;
        }
    }
}
