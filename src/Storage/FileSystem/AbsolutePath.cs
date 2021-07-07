using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace DotNetRu.Auditor.Storage.FileSystem
{
    [DebuggerDisplay("{" + nameof(FullName) + "}")]
    public sealed class AbsolutePath : IEquatable<AbsolutePath>, IReadOnlyList<string>
    {
        private static readonly char[] DirectorySeparators = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        private static readonly char[] InvalidPartChars = Path.GetInvalidFileNameChars();

        public static readonly AbsolutePath Root = ResolveCrossPlatformRootDirectory();

        private readonly IReadOnlyList<string> parts;

        private static AbsolutePath ResolveCrossPlatformRootDirectory()
        {
            if (!TryGetRootPath(Environment.CurrentDirectory, out var rootPath))
            {
                throw new InvalidOperationException($"Can't resolve root directory: {Environment.CurrentDirectory}");
            }

            var parts = new[] { rootPath };
            return new AbsolutePath(parts);
        }

        private AbsolutePath(IReadOnlyList<string> parts)
        {
            if (parts.Count == 0)
            {
                throw new ArgumentException($"Path should be not empty");
            }

            FullName = Path.Combine(parts.ToArray());

            if (!Path.IsPathRooted(FullName))
            {
                throw new ArgumentException($"Path should be absolute: {FullName}");
            }

            this.parts = parts;
            Name = parts[^1];
        }

        public string Name { get; }

        public string FullName { get; }

        public bool IsRoot => parts.Count == 1;

        public int Count => parts.Count;

        public AbsolutePath? Parent => IsRoot
            ? null
            : new AbsolutePath(parts.Take(parts.Count - 1).ToList());

        public AbsolutePath TakeParent(int count)
        {
            if (count == 0)
            {
                throw new ArgumentException("Absolute path can't be empty");
            }

            var newParts = parts.Take(count);
            return new AbsolutePath(newParts.ToList());
        }

        public static AbsolutePath Parse(string path) => TryParse(path, out var absolutePath)
            ? absolutePath
            : throw new ArgumentException($"Can't parse path: {path}");

        public static AbsolutePath operator /(AbsolutePath left, string right) => left.Child(right);

        public static bool TryParse(string path, [NotNullWhen(true)] out AbsolutePath? absolutePath)
        {
            if (String.IsNullOrWhiteSpace(path) ||
                !Path.IsPathRooted(path))
            {
                absolutePath = null;
                return false;
            }

            if (!TryGetRootPath(path, out var rootPath))
            {
                absolutePath = null;
                return false;
            }

            if (rootPath.Length >= path.Length)
            {
                absolutePath = new AbsolutePath(new[] { rootPath });
                return true;
            }

            var notRootPath = path.Substring(rootPath.Length);
            var notRootParts = notRootPath.Split(DirectorySeparators, StringSplitOptions.RemoveEmptyEntries);
            if (notRootParts.Any(part => !IsValidPart(part)))
            {
                absolutePath = null;
                return false;
            }

            var parts = new[] { rootPath }.Concat(notRootParts);
            absolutePath = new AbsolutePath(parts.ToList());
            return true;
        }

        public AbsolutePath Child(string right)
        {
            if (!IsValidPart(right))
            {
                throw new InvalidOperationException("Invalid child name");
            }

            var childParts = parts.Concat(new[] { right });
            return new AbsolutePath(childParts.ToList());
        }

        public override string ToString() => FullName;

        public string this[int index] => parts[index];

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<string> GetEnumerator() => parts.GetEnumerator();

        #region Equality Members

        public bool Equals(AbsolutePath? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return FullName == other.FullName;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as AbsolutePath);
        }

        public override int GetHashCode()
        {
            return FullName.GetHashCode();
        }

        public static bool operator ==(AbsolutePath? left, AbsolutePath? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(AbsolutePath? left, AbsolutePath? right)
        {
            return !Equals(left, right);
        }

        #endregion Equality Members

        /// <remarks>
        /// Not for Root part.
        /// </remarks>>
        private static bool IsValidPart(string part)
        {
            if (String.IsNullOrWhiteSpace(part))
            {
                return false;
            }

            return part.All(c => !InvalidPartChars.Contains(c));
        }

        private static bool TryGetRootPath(string path, [NotNullWhen(true)] out string? rootPath)
        {
            rootPath = Path.GetPathRoot(path);
            if (String.IsNullOrEmpty(rootPath))
            {
                rootPath = default;
                return false;
            }

            if (!Path.EndsInDirectorySeparator(rootPath))
            {
                rootPath += Path.DirectorySeparatorChar;
            }

            return true;
        }
    }

    public static class AbsolutePathExtension
    {
        public static bool IsParentDirectoryFor(this AbsolutePath parentDirectory, AbsolutePath childPath) =>
            childPath.FullName.StartsWith(parentDirectory.FullName, StringComparison.Ordinal);
    }
}
