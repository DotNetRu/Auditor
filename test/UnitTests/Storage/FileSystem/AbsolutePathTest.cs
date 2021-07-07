using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNetRu.Auditor.Storage.FileSystem;
using Xunit;

namespace DotNetRu.Auditor.UnitTests.Storage.FileSystem
{
    public sealed class AbsolutePathTest
    {
        [Fact]
        public void ShouldResolveFullName()
        {
            // Arrange
            var path = AbsolutePath.Root / "A" / "B";
            var expectedFullName = Path.Combine(AbsolutePath.Root.FullName, "A", "B");

            // Act
            var actualFullName = path.FullName;

            // Assert
            Assert.Equal(expectedFullName, actualFullName);
        }

        [Fact]
        public void ShouldResolveName()
        {
            // Arrange
            const string expectedName = "B";
            var path = AbsolutePath.Root / "A" / expectedName;

            // Act
            var actualName = path.Name;

            // Assert
            Assert.Equal(expectedName, actualName);
        }

        [Fact]
        public void ShouldResolveLength()
        {
            // Arrange
            var path = AbsolutePath.Root / "A" / "B";

            // Act
            var pathLength = path.Count;

            // Assert
            Assert.Equal(3, pathLength);
        }

        [Fact]
        public void ShouldResolveDefaultRoot()
        {
            // Act
            var root = AbsolutePath.Root;

            // Assert
            Assert.True(root.IsRoot);

            Assert.True(Path.IsPathFullyQualified(root.FullName));

            var rootFullPath = Path.GetFullPath(root.FullName);
            Assert.Equal(root.FullName, rootFullPath);

            ShouldBeRoot(root.FullName);
        }

        [Theory]
        [InlineData(@"A:")]
        [InlineData(@"b:\")]
        [InlineData(@"C:/")]
        [InlineData(@"\")]
        [InlineData(@"/")]
        public void ShouldBeRoot(string path)
        {
            // Arrange
            const char volumeSeparatorChar = ':';
            static bool IsValidDriveChar(char value) => value is >= 'A' and <= 'Z' or >= 'a' and <= 'z';
            static bool IsDirectorySeparatorChar(char value) => value == Path.DirectorySeparatorChar || value == Path.AltDirectorySeparatorChar;
            static bool IsUnixRoot(string path) => path.Length == 1 && IsDirectorySeparatorChar(path[0]);
            static bool IsWindowsRoot(string path) =>
                path.Length == 3 && IsValidDriveChar(path[0]) && path[1] == volumeSeparatorChar && IsDirectorySeparatorChar(path[2]);

            // Act
            var root = AbsolutePath.Parse(path);
            var rootFullName = root.FullName;

            // Assert
            Assert.True(root.IsRoot);

            Assert.NotEmpty(rootFullName);
            Assert.True(Path.IsPathRooted(rootFullName));
            Assert.Equal(rootFullName, root.Name);
            Assert.Null(root.Parent);

            var part = Assert.Single(root);
            Assert.Equal(rootFullName, part);

            var isNix = IsUnixRoot(rootFullName);
            var isWin = IsWindowsRoot(rootFullName);
            Assert.True(isNix ^ isWin);

            var rootPath = Path.GetPathRoot(rootFullName);
            Assert.Equal(rootFullName, rootPath);
        }

        [Theory]
        [MemberData(nameof(GetDataForParse))]
        public void ShouldTryParse(string path, string expectedPath)
        {
            // Act
            var canParse = AbsolutePath.TryParse(path, out var absolutePath);

            // Assert
            Assert.True(canParse);
            Assert.Equal(Normalize(expectedPath), Normalize(absolutePath?.FullName));
        }

        [Fact]
        public void ShouldParse()
        {
            // Arrange
            string path = Path.Combine("D:", "temp");

            // Act
            var absolutePath = AbsolutePath.Parse(path);

            // Assert
            Assert.Equal(path, absolutePath.FullName);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(" \t\n ")]
        [InlineData(":")]
        [InlineData("A")]
        [InlineData("etc")]
        [InlineData(@"A\B\C")]
        [InlineData("a/b/c")]
        [InlineData("/r??t")]
        [InlineData("/r*t")]
        public void ShouldTryNotParse(string path)
        {
            // Act
            var canParse = AbsolutePath.TryParse(path, out _);

            // Assert
            Assert.False(canParse);
        }

        [Fact]
        public void ShouldNotParse()
        {
            // Arrange
            const string path = "(.)(.)";

            // Act
            AbsolutePath NotParse() => AbsolutePath.Parse(path);

            // Assert
            Assert.Throws<ArgumentException>(NotParse);
        }

        [Fact]
        public void ShouldCombine()
        {
            // Arrange
            var expectedPath = Path.Combine(AbsolutePath.Root.FullName, "A", "B", "C");

            // Act
            var actualPath = AbsolutePath.Root.Child("A").Child("B").Child("C").FullName;

            // Assert
            Assert.Equal(expectedPath, actualPath);
        }

        [Fact]
        public void ShouldBeNotRoot()
        {
            // Arrange
            var path = AbsolutePath.Root / "A";

            // Act
            var isRoot = path.IsRoot;

            // Assert
            Assert.False(isRoot);
        }

        [Fact]
        public void ShouldBeEqual()
        {
            // Arrange
            var path1 = AbsolutePath.Root / "A" / "B";
            var path2Source = Path.Combine(AbsolutePath.Root.FullName, "A", "B") + Path.DirectorySeparatorChar;
            var path2 = AbsolutePath.TryParse(path2Source, out var parsedPath) ? parsedPath : null;

            // Act
            var isEquals = path1.Equals(path2);
            var isObjectEquals = path1.Equals((object?)path2);
            var isHashCodeEquals = path1.GetHashCode() == path2?.GetHashCode();
            var isOperatorEquals = path1 == path2;

            // Assert
            Assert.True(isEquals);
            Assert.True(isObjectEquals);
            Assert.True(isHashCodeEquals);
            Assert.True(isOperatorEquals);
            Assert.Equal(path1, path2);
        }

        [Fact]
        public void ShouldBeNotEqual()
        {
            // Arrange
            var path1 = AbsolutePath.Root / "A" / "B";
            var path2 = AbsolutePath.Root / "A" / "C";

            // Act
            var isNotEquals = !path1.Equals(path2);
            var isObjectNotEquals = !path1.Equals((object?)path2);
            var isHashCodeNotEquals = path1.GetHashCode() != path2.GetHashCode();
            var isOperatorNotEquals = path1 != path2;

            // Assert
            Assert.True(isNotEquals);
            Assert.True(isObjectNotEquals);
            Assert.True(isHashCodeNotEquals);
            Assert.True(isOperatorNotEquals);
            Assert.NotEqual(path1, path2);
        }

        [Fact]
        public void ShouldFormat()
        {
            // Arrange
            var path = AbsolutePath.Root / "A" / "B";

            // Act
            var format = path.ToString();

            // Assert
            Assert.Equal(path.FullName, format);
        }

        [Fact]
        public void ShouldResolveParent()
        {
            // Arrange
            var path = AbsolutePath.Root / "A" / "B";
            var expectedParent = AbsolutePath.Root / "A";

            // Act
            var actualParent = path.Parent;

            // Assert
            Assert.Equal(expectedParent, actualParent);
        }

        [Fact]
        public void ShouldNotResolveParent()
        {
            // Act
            var actualParent = AbsolutePath.Root.Parent;

            // Assert
            Assert.Null(actualParent);
        }

        [Fact]
        public void ShouldTakeParent()
        {
            // Arrange
            var path = AbsolutePath.Root / "A" / "B" / "C";
            var expectedPath = Path.Combine(AbsolutePath.Root.Name, "A");

            // Act
            var parentPath = path.TakeParent(2);

            // Assert
            Assert.Equal(expectedPath, parentPath.FullName);
        }

        [Fact]
        public void ShouldEnumerateAllParts()
        {
            // Arrange
            var path = AbsolutePath.Root / "A" / "B" / "C";
            var expectedParts = new[] { AbsolutePath.Root.FullName, "A", "B", "C" };

            // Act
            var actualParts = path.ToList();

            // Assert
            Assert.Equal(expectedParts, actualParts);
        }

        [Fact]
        public void ShouldIndexAllParts()
        {
            // Act
            var path = AbsolutePath.Root / "A" / "B" / "C";

            // Assert
            Assert.Equal(AbsolutePath.Root.Name, path[0]);
            Assert.Equal("A", path[1]);
            Assert.Equal("B", path[2]);
            Assert.Equal("C", path[3]);
        }

        public static IEnumerable<string[]> GetDataForParse()
        {
            var paths = new[]
            {
                AbsolutePath.Root.FullName,
                "/etc",
                Path.Combine("D:", "temp"),
                Path.Combine("/etc", "passwd"),
            };

            yield return new[] { @"D:", @"D:\" };
            yield return new[] { @"D:\", @"D:\" };

            foreach (var path in paths)
            {
                yield return new[] { path, path };
            }

            foreach (var path in paths)
            {
                var pathWithTail = path + Path.DirectorySeparatorChar;
                yield return new[] { pathWithTail, path };
            }
        }

        private static string? Normalize(string? path) => path?.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
    }
}
