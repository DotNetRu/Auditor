using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetRu.Auditor.Data;

namespace DotNetRu.Auditor.Storage
{
    // TDO: Add Attachments and Blobs
    public interface IReadOnlySession
    {
        Task<T?> LoadAsync<T>(string id)
            where T : IDocument;

        Task<IReadOnlyDictionary<string, T>> LoadAsync<T>(IReadOnlyList<string> ids)
            where T : IDocument;

        public IAsyncEnumerable<T> QueryAsync<T>()
            where T : IDocument;
    }

    public interface ISession : IReadOnlySession
    {
        // TDO: Write methods
        Task AddAsync<T>(T document) // Track? Register? Add? Store?
            where T : IDocument;

        // Task Delete<T>(T document);
        // Task Delete(string id); // Remove?
    }

    public interface IUnitOfWork
    {
        Task SaveChangesAsync();
    }
}
