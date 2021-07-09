using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DotNetRu.Auditor.Storage.IO
{
    /// <remarks>
    /// System.IO.Path class is platform-dependent. Behavior of its methods can be changed on different OSes.
    ///
    /// AbsolutePath has a different philosophy.
    /// It is allows you to manipulate the path in predictable way, regardless of OSes and runtimes.
    ///
    /// TODO:
    ///
    ///  - Maybe support URI
    ///  - Maybe support: UNC, Long UNC (more https://en.wikipedia.org/wiki/Path_(computing))
    ///  - Should we resolve special directories (like: «.», «..», «~»)?
    ///    (https://github.com/dotnet/corert/blob/master/src/System.Private.CoreLib/shared/System/IO/PathInternal.cs#L128)
    /// </remarks>>
    [DebuggerDisplay("{" + nameof(FullName) + "}")]
    public sealed class AbsolutePath : IEquatable<AbsolutePath>, IReadOnlyList<string>
    {
        public static readonly AbsolutePath Root = ResolveCrossPlatformRootDirectory();

        private readonly IReadOnlyList<string> parts;
        private readonly PathNamingStrategy namingStrategy;

        private static AbsolutePath ResolveCrossPlatformRootDirectory()
        {
            if (PathNamingStrategy.TryResolve(Environment.CurrentDirectory, out var nameStrategy, out var pathRoot))
            {
                var parts = new[] { pathRoot };
                return new AbsolutePath(parts, nameStrategy);
            }

            throw new InvalidOperationException($"Can't resolve root volume for {Environment.CurrentDirectory}");
        }

        private AbsolutePath(IReadOnlyList<string> parts, PathNamingStrategy namingStrategy)
        {
            if (parts.Count == 0)
            {
                throw new ArgumentException("Parts should be not empty");
            }

            this.parts = parts;
            this.namingStrategy = namingStrategy;

            Name = parts[^1];
            FullName = namingStrategy.Combine(parts);

            Debug.Assert(namingStrategy.IsPathRooted(FullName));
            Debug.Assert(parts.Skip(1).All(namingStrategy.IsValidName));
        }

        public string Name { get; }

        public string FullName { get; }

        public bool IsRoot => parts.Count == 1;

        public int Count => parts.Count;

        public AbsolutePath? Parent => IsRoot ? null : TakeParent(Count - 1);

        public static AbsolutePath operator /(AbsolutePath left, string right) => left.Child(right);

        public static AbsolutePath Parse(string path) => TryParse(path, out var absolutePath)
            ? absolutePath
            : throw new ArgumentException($"Can't parse path: {path}");

        public static bool TryParse(string path, [NotNullWhen(true)] out AbsolutePath? absolutePath)
        {
            if (String.IsNullOrWhiteSpace(path) ||
                !PathNamingStrategy.TryResolve(path, out var nameStrategy, out var rootPath))
            {
                absolutePath = null;
                return false;
            }

            if (rootPath.Length >= path.Length)
            {
                absolutePath = new AbsolutePath(new[] { rootPath }, nameStrategy);
                return true;
            }

            var notRootPath = path[rootPath.Length..];
            var notRootParts = PathNamingStrategy.Split(notRootPath);
            if (notRootParts.Any(part => !nameStrategy.IsValidName(part)))
            {
                absolutePath = null;
                return false;
            }

            var parts = new[] { rootPath }.Concat(notRootParts);
            absolutePath = new AbsolutePath(parts.ToList(), nameStrategy);
            return true;
        }

        public AbsolutePath TakeParent(int count)
        {
            if (count == 0)
            {
                throw new ArgumentException("Absolute path can't be empty");
            }

            var newParts = parts.Take(count);
            return new AbsolutePath(newParts.ToList(), namingStrategy);
        }

        public AbsolutePath Child(string right)
        {
            if (!namingStrategy.IsValidName(right))
            {
                throw new InvalidOperationException("Invalid child name");
            }

            var childParts = parts.Concat(new[] { right });
            return new AbsolutePath(childParts.ToList(), namingStrategy);
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
    }
}
