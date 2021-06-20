using System;
using System.IO;
using System.Threading.Tasks;

namespace DotNetRu.Auditor.Storage.FileSystem
{
    public abstract class FileSystemEntry : IFileSystemEntry
    {
        protected FileSystemEntry(string fullName)
        {
            FullName = Path.GetFullPath(fullName);
            Name = Path.GetFileName(FullName);
        }

        public string Name { get; }

        public string FullName { get; }

        public abstract ValueTask<bool> ExistsAsync();

        protected string GetFullChildPath(string childName)
        {
            var childPath = Path.Combine(FullName, childName);
            var fullChildPath = Path.GetFullPath(childPath);

            if (!fullChildPath.StartsWith(FullName, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Child path «{fullChildPath}» should be under the root «{FullName}»");
            }

            return fullChildPath;
        }

        protected FileNotFoundException FileNotFound() => new FileNotFoundException($"Could not find file: {FullName}", FullName);
    }
}
