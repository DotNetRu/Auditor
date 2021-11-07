using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetRu.Auditor.Data;

namespace DotNetRu.Auditor.Storage.Sessions
{
    internal interface IDataSession : ISession
    {
        Task WriteAsync<T>(IReadOnlyList<T> documents)
            where T : IDocument;

        Task DeleteAsync<T>(IReadOnlyList<T> documents)
            where T : IDocument;
    }
}
