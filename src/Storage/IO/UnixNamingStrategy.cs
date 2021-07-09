using System.Diagnostics.CodeAnalysis;

namespace DotNetRu.Auditor.Storage.IO
{
    internal sealed class UnixNamingStrategy : PathNamingStrategy
    {
        public static readonly UnixNamingStrategy Instance = new();

        private const string VolumeSeparator = "/";
        private const char VolumeSeparatorChar = '/';

        public override char DirectorySeparatorChar => '/';

        // Copied from https://github.com/dotnet/corert/blob/master/src/System.Private.CoreLib/shared/System/IO/Path.Unix.cs
        protected override char[] InvalidFileNameChars { get; } = { '\0', '/' };

        public override bool TryGetPathRoot(string path, [NotNullWhen(true)] out string? root)
        {
            if (path.Length > 0 && path[0] == VolumeSeparatorChar)
            {
                root = VolumeSeparator;
                return true;
            }

            root = default;
            return false;
        }
    }
}
