using System;
using System.IO;

namespace DotNetRu.Auditor.Storage.FileSystem
{
    public abstract class FileSystemEntry : IFileSystemEntry
    {
        protected FileSystemEntry(string fullName, bool exists)
        {
            FullName = Path.GetFullPath(fullName);
            Name = Path.GetFileName(FullName);
            Exists = exists;
        }

        public string Name { get; }

        public string FullName { get; }

        public bool Exists { get; }

        protected string GetFullPath(string subPath)
        {
            var childrenPath = Path.Combine(FullName, subPath);
            var childrenFullPath = Path.GetFullPath(childrenPath);

            if (!childrenFullPath.StartsWith(FullName, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Children path «{childrenFullPath}» should be under the root «{FullName}»");
            }

            return childrenFullPath;
        }
    }
}
