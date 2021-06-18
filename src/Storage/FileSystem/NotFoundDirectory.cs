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

        public static ValueTask<IDirectory> ToTask(string fullName) => ValueTask.FromResult<IDirectory>(new NotFoundDirectory(fullName));

        public ValueTask<IDirectory> GetDirectoryInfoAsync(string subPath) => ToTask(subPath);

        public ValueTask<IFile> GetFileInfoAsync(string subPath) => NotFoundFile.ToTask(subPath);

        public IAsyncEnumerable<IDirectory> EnumerateDirectoriesAsync() => AsyncEnumerable.Empty<IDirectory>();

        public IAsyncEnumerable<IFile> EnumerateFilesAsync() => AsyncEnumerable.Empty<IFile>();

        public ValueTask<IFile> CreateFileAsync(string subPath)
        {
            throw new System.NotImplementedException();
        }
    }
}
