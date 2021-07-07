using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DotNetRu.Auditor.Storage.FileSystem.Memory;
using DotNetRu.Auditor.Storage.FileSystem.PathEngine;

namespace DotNetRu.Auditor.Storage.FileSystem
{
    public sealed class MemoryFileSystem : IFileSystem
    {
        private readonly MemoryFileTable entries = new();

        public static IDirectory ForDirectory(string path)
        {
            var fileSystem = new MemoryFileSystem();
            return fileSystem.GetDirectory(path);
        }

        public IDirectory GetDirectory(string path)
        {
            var directory = new PathDirectory(path, this);
            return directory;
        }

        async IAsyncEnumerable<string> IFileSystem.EnumerateDirectoriesAsync(string path)
        {
            await Task.Yield();

            foreach (var directoryPath in entries.EnumerateEntries(path, false))
            {
                yield return directoryPath;
            }
        }

        async IAsyncEnumerable<string> IFileSystem.EnumerateFilesAsync(string path)
        {
            await Task.Yield();

            foreach (var filePath in entries.EnumerateEntries(path, true))
            {
                yield return filePath;
            }
        }

        ValueTask<bool> IFileSystem.DirectoryExistsAsync(string path)
        {
            var exists = entries.DirectoryExists(path);
            return ValueTask.FromResult(exists);
        }

        ValueTask<bool> IFileSystem.FileExistsAsync(string path)
        {
            var exists = entries.FileExists(path);
            return ValueTask.FromResult(exists);
        }

        ValueTask<bool> IFileSystem.RequestWriteAccessForFileAsync(string path)
        {
            return ValueTask.FromResult(true);
        }

        ValueTask<Stream> IFileSystem.OpenFileForReadAsync(string path)
        {
            if (!entries.TryGetFile(path, out var file))
            {
                throw FileNotFound(path);
            }

            var inputStream = file.Content;
            inputStream.Seek(0, SeekOrigin.Begin);
            return ValueTask.FromResult<Stream>(inputStream);
        }

        ValueTask<Stream> IFileSystem.OpenFileForWriteAsync(string path)
        {
            var file = entries.GetOrCreateFile(path);
            var outputStream = file.Content;
            outputStream.Seek(0, SeekOrigin.Begin);
            return ValueTask.FromResult<Stream>(outputStream);
        }

        ValueTask<bool> IFileSystem.DeleteFileAsync(string path)
        {
            var deleted = entries.DeleteFile(path);
            return ValueTask.FromResult(deleted);
        }

        private static FileNotFoundException FileNotFound(string path) => new($"Could not find file: {path}", path);
    }
}
