using System;

namespace DotNetRu.Auditor.Storage.FileSystem
{
    public class NullFileSystem : NotFoundDirectory, IFileSystem
    {
        public static readonly NullFileSystem Instance = new NullFileSystem();

        public NullFileSystem()
            : base(String.Empty)
        {
        }
    }
}
