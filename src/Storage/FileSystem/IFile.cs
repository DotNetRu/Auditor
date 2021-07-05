using System.IO;
using System.Threading.Tasks;

namespace DotNetRu.Auditor.Storage.FileSystem
{
    public interface IFile : IFileSystemEntry
    {
        Task<Stream> OpenForReadAsync();

        Task<IWritableFile?> RequestWriteAccessAsync();
    }
}
