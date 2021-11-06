using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetRu.Auditor.Data;

namespace DotNetRu.Auditor.Storage.Sessions
{
    internal class CacheableSession : ISession, IUnitOfWork
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
            if (map.TryGet(id, out var cachedDocument))
            {
                return cachedDocument;
            }

            var document = await dataSession.LoadAsync<T>(id).ConfigureAwait(false);

            return document?.Id == null ? default : map.RegisterOrigin(document);
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

            var documents = new Dictionary<string, T>(cachedDocuments);
            var newIds = ids.Except(documents.Keys).ToList();
            var newDocuments = await dataSession.LoadAsync<T>(newIds).ConfigureAwait(false);

            foreach (var (_, newDocument) in newDocuments)
            {
                if (newDocument.Id != null)
                {
                    var registeredDocument = map.RegisterOrigin(newDocument);
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
            // TODO: Perf: Check all document **IDs** in the map before query (before serialize)
            var map = GetIdentityMap<T>();

            await foreach (var document in dataSession.QueryAsync<T>().ConfigureAwait(false))
            {
                if (document.Id != null)
                {
                    var registeredDocument = map.RegisterOrigin(document);
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

        public Task SaveChangesAsync()
        {
            // TDO: Refactor for testability
            // TDO: Process all states
            // TDO: Do not write twice
            // [x] Unchanged — skipp
            // [x] Modified — write
            // [x] Added — write
            // [ ] Deleted - remove

            return dataCache
                .GetAllMaps()
                // HACK: Dynamic dispatch
                .Select(map => (Task)SaveMapAsync((dynamic)map))
                .WhenAll();
        }

        private Task SaveMapAsync<T>(IdentityMap<T> map)
            where T : IDocument
        {
            var documents = map.PopChanges();
            return dataSession.WriteAsync(documents);
        }

        private IdentityMap<T> GetIdentityMap<T>()
            where T : IDocument =>
            dataCache.GetMap<T>();
    }
}
