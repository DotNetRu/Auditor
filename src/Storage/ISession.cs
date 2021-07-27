using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetRu.Auditor.Data;

namespace DotNetRu.Auditor.Storage
{
    // TDO: Add blob access
    public interface ISession
    {
        Task<T?> LoadAsync<T>(string id)
            where T : IRecord;

        Task<IReadOnlyDictionary<string, T>> LoadAsync<T>(IReadOnlyList<string> ids)
            where T : IRecord;

        public IAsyncEnumerable<T> QueryAsync<T>()
            where T : IRecord;

        // TDO: Write methods
        // void Save<T>(T record) // Add? Store? Create? Register? Track?
        //     where T : IRecord;
        //
        // Task Delete(string id); // Remove?
        //
        // Task CommitChangesAsync(CancellationToken token = default);
    }
}
