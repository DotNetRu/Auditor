using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetRu.Auditor.Data;

namespace DotNetRu.Auditor.Storage.Sessions
{
    internal class CacheableSession : IUnitOfWork
    {
        private readonly IDataSession dataSession;
        private readonly IDataCache dataCache;

        public CacheableSession(IDataSession dataSession, IDataCache? dataCache = null)
        {
            this.dataSession = dataSession;
            this.dataCache = dataCache ?? new DataCache();
        }

        public async Task<T?> LoadAsync<T>(string id)
            where T : IDocument
        {
            var map = GetIdentityMap<T>();
            var status = map.Resolve(id);

            switch (status)
            {
                case { Cache: { }, IsDeleted: false }:
                    return status.Cache;
                case { Cache: null, IsDeleted: true }:
                    return default;
            }

            var document = await dataSession.LoadAsync<T>(id).ConfigureAwait(false);
            if (document == null)
            {
                return default;
            }

            status = map.RegisterOrigin(document);
            return status.Cache;
        }

        public async Task<IReadOnlyDictionary<string, T>> LoadAsync<T>(IReadOnlyList<string> ids)
            where T : IDocument
        {
            var map = GetIdentityMap<T>();
            var (cache, deleted) = map.Resolve(ids);

            if (cache.Count + deleted.Count == ids.Count)
            {
                // We found all
                return cache;
            }

            var documents = new Dictionary<string, T>(cache);
            var newIds = ids.Except(documents.Keys).Except(deleted.Keys).ToList();
            var newDocuments = await dataSession.LoadAsync<T>(newIds).ConfigureAwait(false);

            foreach (var (_, newDocument) in newDocuments)
            {
                var registeredDocument = map.RegisterOrigin(newDocument).Cache;
                if (registeredDocument?.Id != null)
                {
                    documents.Add(registeredDocument.Id, registeredDocument);
                }
            }

            return documents;
        }

        public async IAsyncEnumerable<T> QueryAsync<T>()
            where T : IDocument
        {
            // TODO: Perf: Check all document **IDs** in the map before query (before serialize)
            var map = GetIdentityMap<T>();

            await foreach (var document in dataSession.QueryAsync<T>().ConfigureAwait(false))
            {
                var registeredDocument = map.RegisterOrigin(document).Cache;
                if (registeredDocument != null)
                {
                    yield return registeredDocument;
                }
            }
        }

        public Task AddAsync<T>(T document)
            where T : IDocument
        {
            var map = GetIdentityMap<T>();

            map.RegisterNew(document);

            return Task.CompletedTask;
        }

        public Task DeleteAsync<T>(T document)
            where T : IDocument
        {
            var map = GetIdentityMap<T>();

            map.RegisterDeleted(document);

            return Task.CompletedTask;
        }

        public Task SaveChangesAsync()
        {
            return dataCache
                .GetAllMaps()
                // HACK: Dynamic dispatch
                .Select(map => (Task)SaveMapAsync((dynamic)map))
                .WhenAll();
        }

        private async Task SaveMapAsync<T>(IdentityMap<T> map)
            where T : IDocument
        {
            var (writeList, deleteList) = map.PopChanges();

            await dataSession.WriteAsync(writeList).ConfigureAwait(false);
            await dataSession.DeleteAsync(deleteList).ConfigureAwait(false);
        }

        private IdentityMap<T> GetIdentityMap<T>()
            where T : IDocument =>
            dataCache.GetMap<T>();
    }
}
