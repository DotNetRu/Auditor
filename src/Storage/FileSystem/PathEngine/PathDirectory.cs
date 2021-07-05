using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetRu.Auditor.Storage.FileSystem.PathEngine
{
    internal sealed class PathDirectory : FileSystemEntry, IDirectory
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
            await foreach (var directoryFullName in fileSystem.EnumerateDirectoriesAsync(FullName))
            {
                yield return new PathDirectory(directoryFullName, fileSystem);
            }
        }

        public async IAsyncEnumerable<IFile> EnumerateFilesAsync()
        {
            await foreach (var fileFullName in fileSystem.EnumerateFilesAsync(FullName))
            {
                yield return new PathFile(fileFullName, fileSystem);
            }
        }
    }
}
