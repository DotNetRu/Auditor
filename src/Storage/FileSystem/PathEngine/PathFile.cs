using System.IO;
using System.Threading.Tasks;

namespace DotNetRu.Auditor.Storage.FileSystem.PathEngine
{
    internal sealed class PathFile : FileSystemEntry, IWritableFile
    {
        private readonly IFileSystem fileSystem;

        public PathFile(string path, IFileSystem fileSystem)
            : base(path)
        {
            this.fileSystem = fileSystem;
        }

        public override ValueTask<bool> ExistsAsync() => fileSystem.FileExistsAsync(FullName);

        public async Task<Stream> OpenForReadAsync() => await fileSystem.OpenFileForReadAsync(FullName).ConfigureAwait(false);

        public async Task<Stream> OpenForWriteAsync() => await fileSystem.OpenFileForWriteAsync(FullName).ConfigureAwait(false);

        public async Task<bool> DeleteAsync() => await fileSystem.DeleteFileAsync(FullName).ConfigureAwait(false);

        public async Task<IWritableFile?> RequestWriteAccessAsync()
        {
            var accessGranted = await fileSystem.RequestWriteAccessForFileAsync(FullName).ConfigureAwait(false);
            return accessGranted ? this : null;
        }
    }
}
