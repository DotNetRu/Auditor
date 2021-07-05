using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DotNetRu.Auditor.Storage.FileSystem.PathEngine;

namespace DotNetRu.Auditor.Storage.FileSystem
{
    public sealed class PhysicalFileSystem : IFileSystem
    {
        // HACK: Resolve cross-platform root directory
        public static readonly string RootPath = Path.GetPathRoot(Environment.CurrentDirectory) ?? throw new InvalidOperationException("Unknown root directory");

        public static IDirectory ForDirectory(string path)
        {
            var fileSystem = new PhysicalFileSystem();
            return fileSystem.GetDirectory(path);
        }

        public IDirectory GetDirectory(string path)
        {
            var directory = new PathDirectory(path, this);
            return directory;
        }

        async IAsyncEnumerable<string> IFileSystem.EnumerateDirectoriesAsync(string path)
        {
            var exists = Directory.Exists(path);
            if (!exists)
            {
                yield break;
            }

            await Task.Yield();

            foreach (var directoryFullName in Directory.EnumerateDirectories(path))
            {
                yield return directoryFullName;
            }
        }

        async IAsyncEnumerable<string> IFileSystem.EnumerateFilesAsync(string path)
        {
            var exists = Directory.Exists(path);
            if (!exists)
            {
                yield break;
            }

            await Task.Yield();

            foreach (var fileFullName in Directory.EnumerateFiles(path))
            {
                yield return fileFullName;
            }
        }

        ValueTask<bool> IFileSystem.DirectoryExistsAsync(string path)
        {
            var exists = Directory.Exists(path);
            return ValueTask.FromResult(exists);
        }

        ValueTask<bool> IFileSystem.FileExistsAsync(string path)
        {
            var exists = File.Exists(path);
            return ValueTask.FromResult(exists);
        }

        ValueTask<bool> IFileSystem.RequestWriteAccessForFileAsync(string path)
        {
            // TODO: Test real file system permissions
            return ValueTask.FromResult(true);
        }

        ValueTask<Stream> IFileSystem.OpenFileForReadAsync(string path)
        {
            var exists = File.Exists(path);
            if (!exists)
            {
                throw FileNotFound(path);
            }

            var inputStream = File.OpenRead(path);
            return ValueTask.FromResult<Stream>(inputStream);
        }

        ValueTask<Stream> IFileSystem.OpenFileForWriteAsync(string path)
        {
            EnsureParentDirectoryExists(path);

            var outputStream = File.OpenWrite(path);
            return ValueTask.FromResult<Stream>(outputStream);
        }

        ValueTask<bool> IFileSystem.DeleteFileAsync(string path)
        {
            var exists = File.Exists(path);
            if (!exists)
            {
                return ValueTask.FromResult(false);
            }

            try
            {
                File.Delete(path);
            }
            catch (Exception)
            {
                // Result will be checked later
            }

            exists = File.Exists(path);
            return ValueTask.FromResult(!exists);
        }

        private static void EnsureParentDirectoryExists(string path)
        {
            var parentDirectoryPath = Path.GetDirectoryName(path);
            if (parentDirectoryPath != null && !Directory.Exists(parentDirectoryPath))
            {
                Directory.CreateDirectory(parentDirectoryPath);
            }
        }

        private static FileNotFoundException FileNotFound(string path) => new($"Could not find file: {path}", path);
    }
}
