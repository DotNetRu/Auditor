using System.Diagnostics.CodeAnalysis;

namespace DotNetRu.Auditor.Storage.IO
{
    internal sealed class WindowsNamingStrategy : PathNamingStrategy
    {
        public static readonly WindowsNamingStrategy Instance = new();

        private const char VolumeSeparatorChar = ':';
        private const char AltDirectorySeparatorChar = '/';

        public override char DirectorySeparatorChar => '\\';

        // Copied from https://github.com/dotnet/corert/blob/master/src/System.Private.CoreLib/shared/System/IO/Path.Windows.cs
        protected override char[] InvalidFileNameChars { get; } =
        {
            '\"', '<', '>', '|', '\0',
            (char)1, (char)2, (char)3, (char)4, (char)5, (char)6, (char)7, (char)8, (char)9, (char)10,
            (char)11, (char)12, (char)13, (char)14, (char)15, (char)16, (char)17, (char)18, (char)19, (char)20,
            (char)21, (char)22, (char)23, (char)24, (char)25, (char)26, (char)27, (char)28, (char)29, (char)30,
            (char)31, ':', '*', '?', '\\', '/'
        };

        public override bool TryGetPathRoot(string path, [NotNullWhen(true)] out string? root)
        {
            if (path.Length > 1 &&
                path[1] == VolumeSeparatorChar &&
                IsValidDriveChar(path[0]))
            {
                if (path.Length == 2 || IsDirectorySeparator(path[2]))
                {
                    // We add a directory separator, even if it wasn't there.
                    // Because sometimes the root disk without a separator is treated incorrectly.
                    // For example: `Path.GetFullPath("C:")`
                    // Will return you "C:\full-path-to-your-current-directory" instead of "C:\".

                    // We use a backslash instead of the original letter for consistency.
                    // Because all the other parts of the path will use a backslash.
                    // Windows supports both options.
                    root = path[..2] + DirectorySeparatorChar;
                    return true;
                }
            }

            root = default;
            return false;
        }

        private static bool IsValidDriveChar(char value)
        {
            return value is >= 'A' and <= 'Z' or >= 'a' and <= 'z';
        }

        private bool IsDirectorySeparator(char c)
        {
            // Windows supports both forward slash and backslash in paths
            return c == DirectorySeparatorChar || c == AltDirectorySeparatorChar;
        }
    }
}
