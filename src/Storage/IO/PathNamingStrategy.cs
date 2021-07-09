using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace DotNetRu.Auditor.Storage.IO
{
    internal abstract class PathNamingStrategy
    {
        private static readonly IReadOnlyList<PathNamingStrategy> AllStrategies = new PathNamingStrategy[]
        {
            UnixNamingStrategy.Instance,
            WindowsNamingStrategy.Instance
        };

        public abstract char DirectorySeparatorChar { get; }

        // We don't copy the array for performance reasons. This is possible because it's non-public class.
        protected abstract char[] InvalidFileNameChars { get; }

        private static readonly char[] AllDirectorySeparatorChars = AllStrategies
            .Select(strategy => strategy.DirectorySeparatorChar)
            .ToArray();

        public static bool TryResolve(string path, [NotNullWhen(true)] out PathNamingStrategy? nameStrategy) =>
            TryResolve(path, out nameStrategy, out _);

        internal static bool TryResolve(
            string path,
            [NotNullWhen(true)] out PathNamingStrategy? nameStrategy,
            [NotNullWhen(true)] out string? root)
        {
            foreach (var strategy in AllStrategies)
            {
                if (strategy.TryGetPathRoot(path, out root))
                {
                    nameStrategy = strategy;
                    return true;
                }
            }

            nameStrategy = default;
            root = default;
            return false;
        }

        public static IReadOnlyList<string> Split(string path)
        {
            return path.Split(AllDirectorySeparatorChars, StringSplitOptions.RemoveEmptyEntries);
        }

        public bool IsPathRooted(string path) => TryGetPathRoot(path, out _);

        public abstract bool TryGetPathRoot(string path, [NotNullWhen(true)] out string? root);

        public string Combine(IReadOnlyList<string> paths)
        {
            var builder = new StringBuilder();

            var parentHasSeparator = true;
            foreach (var path in paths)
            {
                if (path == String.Empty)
                {
                    continue;
                }

                // TODO: Start with new Root if any part is rooted (like Path.Combine("A", "/"))
                if (!parentHasSeparator)
                {
                    builder.Append(DirectorySeparatorChar);
                }

                builder.Append(path);

                parentHasSeparator = path[^1] == DirectorySeparatorChar;
            }

            return builder.ToString();
        }

        /// <remarks>
        /// Not for Root part.
        /// </remarks>>
        public bool IsValidName(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            var invalidChars = InvalidFileNameChars;
            return !Array.Exists(invalidChars, name.Contains);
        }
    }
}
