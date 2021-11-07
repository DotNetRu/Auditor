using System.Threading.Tasks;

namespace DotNetRu.Auditor.Storage.FileSystem
{
    public interface IWritableDirectory : IDirectory
    {
        Task<bool> DeleteAsync();
    }
}
