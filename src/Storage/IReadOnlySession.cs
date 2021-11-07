using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetRu.Auditor.Data;

namespace DotNetRu.Auditor.Storage
{
    public interface IReadOnlySession
    {
        Task<T?> LoadAsync<T>(string id)
            where T : IDocument;

        Task<IReadOnlyDictionary<string, T>> LoadAsync<T>(IReadOnlyList<string> ids)
            where T : IDocument;

        IAsyncEnumerable<T> QueryAsync<T>()
            where T : IDocument;
    }
}
