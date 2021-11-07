using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DotNetRu.Auditor.Storage.IO;

namespace DotNetRu.Auditor.Storage.FileSystem.Memory
{
    internal sealed class MemoryFileTable
    {
        private readonly ConcurrentDictionary<AbsolutePath, MemoryFile> allFiles = new();

        public bool DirectoryExists(string path)
        {
            var directoryPath = AbsolutePath.Parse(path);

            return EnumerateDirectoryContent(directoryPath).Any();
        }

        public bool FileExists(string path)
        {
            var filePath = AbsolutePath.Parse(path);

            return allFiles.ContainsKey(filePath);
        }

        public bool TryGetFile(string path, [NotNullWhen(true)] out MemoryFile? file)
        {
            var filePath = AbsolutePath.Parse(path);

            return allFiles.TryGetValue(filePath, out file);
        }

        public MemoryFile GetOrCreateFile(string path)
        {
            var filePath = AbsolutePath.Parse(path);

            return allFiles.GetOrAdd(filePath, _ => new MemoryFile());
        }

        public bool DeleteFile(string path)
        {
            var filePath = AbsolutePath.Parse(path);

            if (allFiles.TryRemove(filePath, out var file))
            {
                file.Content.RealClose();
                return true;
            }

            return false;
        }

        public bool DeleteDirectory(string path)
        {
            var directoryPath = AbsolutePath.Parse(path);

            var filesInDirectory = EnumerateDirectoryContent(directoryPath).ToList();
            if (!filesInDirectory.Any())
            {
                return false;
            }

            foreach (var filePath in filesInDirectory)
            {
                DeleteFile(filePath.FullName);
            }

            return !DirectoryExists(path);
        }

        public IEnumerable<string> EnumerateEntries(string path, bool isFile)
        {
            var directoryPath = AbsolutePath.Parse(path);

            return EnumerateEntriesUnsafe(directoryPath)
                .Where(entry => entry.IsLeaf == isFile)
                .Select(entry => entry.Path.FullName)
                .ToList();
        }

        private IEnumerable<(AbsolutePath Path, bool IsLeaf)> EnumerateEntriesUnsafe(AbsolutePath directoryPath)
        {
            var entryDepth = directoryPath.Count + 1;

            (AbsolutePath Path, bool IsLeaf) PathWithDepth(AbsolutePath filePath) => filePath.Count == entryDepth ?
                (filePath, true) :
                (filePath.TakeParent(entryDepth), false);

            // ReSharper disable once InconsistentlySynchronizedField
            return EnumerateDirectoryContent(directoryPath)
                .Select(PathWithDepth)
                .Distinct();
        }

        private IEnumerable<AbsolutePath> EnumerateDirectoryContent(AbsolutePath directoryPath) =>
            allFiles
                .Keys
                .Where(directoryPath.IsParentDirectoryFor);
    }
}
