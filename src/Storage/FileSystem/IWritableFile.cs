using System.IO;
using System.Threading.Tasks;

namespace DotNetRu.Auditor.Storage.FileSystem
{
    public interface IWritableFile : IFile
    {
        Task<Stream> OpenForWriteAsync();

        Task<bool> DeleteAsync();
    }
}
