using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetRu.Auditor.Storage.FileSystem
{
    public interface IDirectory : IFileSystemEntry
    {
        IDirectory GetDirectory(string childDirectoryName);

        IFile GetFile(string childFileName);

        IAsyncEnumerable<IDirectory> EnumerateDirectoriesAsync();

        IAsyncEnumerable<IFile> EnumerateFilesAsync();

        Task<IWritableDirectory?> RequestWriteAccessAsync();
    }
}
