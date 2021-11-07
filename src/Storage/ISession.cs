using System.Threading.Tasks;
using DotNetRu.Auditor.Data;

namespace DotNetRu.Auditor.Storage
{
    // TDO: Add Attachments and Blobs
    public interface ISession : IReadOnlySession
    {
        Task AddAsync<T>(T document)
            where T : IDocument;

        Task DeleteAsync<T>(T document)
            where T : IDocument;
    }
}
