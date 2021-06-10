using System;
using System.IO;

namespace DotNetRu.Auditor.Storage.FileSystem.Physical
{
    public sealed class PhysicalFileSystem : PhysicalDirectory, IFileSystem
    {
        public PhysicalFileSystem(string root)
            : base(root)
        {
            if (!Path.IsPathRooted(root))
            {
                throw new ArgumentException($"The path «{root}» must be absolute", nameof(root));
            }
        }
    }
}


