using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DotNetRu.Auditor.Storage.FileSystem.Memory
{
    internal sealed class MemoryFileTable
    {
        private readonly ConcurrentDictionary<AbsolutePath, MemoryFile> allFiles = new();

        public bool DirectoryExists(string path)
        {
            var directoryPath = AbsolutePath.Parse(path);

            return allFiles.Keys.Any(directoryPath.IsParentDirectoryFor);
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
            return allFiles
                .Keys
                .Where(directoryPath.IsParentDirectoryFor)
                .Where(filePath => filePath.Count > directoryPath.Count)
                .Select(PathWithDepth)
                .Distinct();
        }
    }
}
