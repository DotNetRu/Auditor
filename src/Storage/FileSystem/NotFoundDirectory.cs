using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetRu.Auditor.Storage.FileSystem
{
    public class NotFoundDirectory : FileSystemEntry, IDirectory
    {
        public NotFoundDirectory(string fullName)
            : base(fullName, false)
        {
        }

        public static Task<IDirectory> ToTask(string fullName) => Task.FromResult<IDirectory>(new NotFoundDirectory(fullName));

        public Task<IDirectory> GetDirectoryInfoAsync(string subPath) => ToTask(subPath);

        public Task<IFile> GetFileInfoAsync(string subPath) => NotFoundFile.ToTask(subPath);

        public IAsyncEnumerable<IDirectory> EnumerateDirectoriesAsync() => AsyncEnumerable.Empty<IDirectory>();

        public IAsyncEnumerable<IFile> EnumerateFilesAsync() => AsyncEnumerable.Empty<IFile>();
    }
}
