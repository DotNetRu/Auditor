using System;

namespace DotNetRu.Auditor.Storage.IO
{
    public static class AbsolutePathExtension
    {
        public static bool IsParentDirectoryFor(this AbsolutePath parentDirectory, AbsolutePath childPath) =>
            parentDirectory.FullName.Length < childPath.FullName.Length &&
            childPath.FullName.StartsWith(parentDirectory.FullName, StringComparison.Ordinal);
    }
}
