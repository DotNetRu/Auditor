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

        public static ValueTask<IDirectory> AsTask(string fullName) => ValueTask.FromResult<IDirectory>(new NotFoundDirectory(fullName));

        public ValueTask<IDirectory> GetDirectoryAsync(string subPath) => AsTask(subPath);

        public ValueTask<IFile> GetFileAsync(string subPath) => NotFoundFile.AsTask(subPath);

        public IAsyncEnumerable<IDirectory> EnumerateDirectoriesAsync() => AsyncEnumerable.Empty<IDirectory>();

        public IAsyncEnumerable<IFile> EnumerateFilesAsync() => AsyncEnumerable.Empty<IFile>();
    }
}
