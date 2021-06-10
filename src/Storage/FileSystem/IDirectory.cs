using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetRu.Auditor.Storage.FileSystem
{
    public interface IDirectory : IFileSystemEntry
    {
        Task<IDirectory> GetDirectoryInfoAsync(string subPath);

        Task<IFile> GetFileInfoAsync(string subPath);

        IAsyncEnumerable<IDirectory> EnumerateDirectoriesAsync();

        IAsyncEnumerable<IFile> EnumerateFilesAsync();
    }
}
