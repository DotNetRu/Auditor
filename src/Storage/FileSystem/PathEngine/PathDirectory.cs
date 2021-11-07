using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetRu.Auditor.Storage.FileSystem.PathEngine
{
    internal sealed class PathDirectory : FileSystemEntry, IWritableDirectory
    {
        private readonly IFileSystem fileSystem;

        public PathDirectory(string path, IFileSystem fileSystem)
            : base(path)
        {
            this.fileSystem = fileSystem;
        }

        public override ValueTask<bool> ExistsAsync() => fileSystem.DirectoryExistsAsync(FullName);

        public IDirectory GetDirectory(string childDirectoryName)
        {
            var fullChildName = GetFullChildPath(childDirectoryName);
            var directory = new PathDirectory(fullChildName, fileSystem);
            return directory;
        }

        public IFile GetFile(string childFileName)
        {
            var fullChildName = GetFullChildPath(childFileName);
            var file = new PathFile(fullChildName, fileSystem);
            return file;
        }

        public async IAsyncEnumerable<IDirectory> EnumerateDirectoriesAsync()
        {
            await foreach (var directoryFullName in fileSystem.EnumerateDirectoriesAsync(FullName).ConfigureAwait(false))
            {
                yield return new PathDirectory(directoryFullName, fileSystem);
            }
        }

        public async IAsyncEnumerable<IFile> EnumerateFilesAsync()
        {
            await foreach (var fileFullName in fileSystem.EnumerateFilesAsync(FullName).ConfigureAwait(false))
            {
                yield return new PathFile(fileFullName, fileSystem);
            }
        }

        public async Task<bool> DeleteAsync() => await fileSystem.DeleteDirectoryAsync(FullName).ConfigureAwait(false);

        public async Task<IWritableDirectory?> RequestWriteAccessAsync()
        {
            var accessGranted = await fileSystem.RequestWriteAccessForDirectoryAsync(FullName).ConfigureAwait(false);
            return accessGranted ? this : null;
        }
    }
}
