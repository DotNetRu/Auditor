using System.Collections.Generic;

namespace DotNetRu.Auditor.Storage.FileSystem
{
    public interface IDirectory : IFileSystemEntry
    {
        IDirectory GetDirectory(string childDirectoryName);

        IFile GetFile(string childFileName);

        IAsyncEnumerable<IDirectory> EnumerateDirectoriesAsync();

        IAsyncEnumerable<IFile> EnumerateFilesAsync();
    }
}
