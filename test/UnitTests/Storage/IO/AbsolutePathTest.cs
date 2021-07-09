using System;
using System.IO;
using System.Linq;
using DotNetRu.Auditor.Storage.IO;
using Xunit;

namespace DotNetRu.Auditor.UnitTests.Storage.IO
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
        public void ShouldResolvePartsCount()
        {
            // Arrange
            var path = AbsolutePath.Root / "A" / "B";

            // Act
            var partsCount = path.Count;

            // Assert
            Assert.Equal(3, partsCount);
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
        public void ShouldResolveDefaultRoot()
        {
            // Act
            var root = AbsolutePath.Root;

            // Assert
            Assert.True(Path.IsPathFullyQualified(root.FullName));
            Assert.True(Path.IsPathRooted(root.FullName));

            var rootFullPath = Path.GetFullPath(root.FullName);
            Assert.Equal(root.FullName, rootFullPath);

            ShouldBeRoot(root.FullName);
        }

        [Theory]
        [InlineData(@"A:")]
        [InlineData(@"b:\")]
        [InlineData(@"C:/")]
        [InlineData(@"/")]
        public void ShouldBeRoot(string path)
        {
            // Act
            var root = AbsolutePath.Parse(path);
            var rootFullName = root.FullName;

            // Assert
            Assert.True(root.IsRoot);

            Assert.NotEmpty(rootFullName);
            Assert.Equal(rootFullName, root.Name);
            Assert.Null(root.Parent);

            var part = Assert.Single(root);
            Assert.Equal(rootFullName, part);

            var parsedRoot = AbsolutePath.Parse(rootFullName).FullName;
            Assert.Equal(rootFullName, parsedRoot);
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

        [Theory]
        [InlineData(@"D:\A")]
        [InlineData(@"D:\A\B\", @"D:\A\B")]
        [InlineData("F:/A", @"F:\A")]
        [InlineData("W:/A/", @"W:\A")]
        [InlineData("Z:/A/B", @"Z:\A\B")]
        [InlineData("/etc")]
        [InlineData("/etc/", "/etc")]
        [InlineData("/etc/passwd")]
        public void ShouldTryParse(string path, string? expectedPath = null)
        {
            // Arrange
            expectedPath ??= path;

            // Act
            var canParse = AbsolutePath.TryParse(path, out var absolutePath);

            // Assert
            Assert.True(canParse);
            Assert.Equal(expectedPath, absolutePath?.FullName);
        }

        [Fact]
        public void ShouldParse()
        {
            // Arrange
            const string path = @"D:\temp";

            // Act
            var absolutePath = AbsolutePath.Parse(path);

            // Assert
            Assert.Equal(path, absolutePath.FullName);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(" \t\n\r ")]
        [InlineData(":")]
        [InlineData("A")]
        [InlineData("etc")]
        [InlineData("a/b/c")]
        [InlineData(@"A\B\C")]
        [InlineData(@"C:\r??t")]
        [InlineData(@"C:\r*t")]
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
        public void ShouldCombineChildDirectory()
        {
            // Arrange
            var expectedPath = Path.Combine(AbsolutePath.Root.FullName, "A", "B", "C");

            // Act
            var actualPath = AbsolutePath.Root.Child("A").Child("B").Child("C").FullName;

            // Assert
            Assert.Equal(expectedPath, actualPath);
        }

        [Fact]
        public void ShouldBeEqual()
        {
            // Arrange
            var path1 = AbsolutePath.Root / "A" / "B";
            var path2Source = Path.Combine(AbsolutePath.Root.FullName, "A", "B");
            var path2 = AbsolutePath.Parse(path2Source);

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
        public void ShouldNotTakeZeroParent()
        {
            // Arrange
            var path = AbsolutePath.Root / "A" / "B" / "C";
            var expectedPath = Path.Combine(AbsolutePath.Root.Name, "A");

            // Act
            AbsolutePath ZeroParent() => path.TakeParent(0);

            // Assert
            Assert.Throws<ArgumentException>(ZeroParent);
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

        [Theory]
        [InlineData("C:", @"C:\temp")]
        [InlineData(@"C:\temp", @"C:\temp\docs")]
        [InlineData("/", "/etc/config")]
        public void ShouldDetectParentDirectoryFor(string parentDirectory, string childPath)
        {
            // Arrange
            var parent = AbsolutePath.Parse(parentDirectory);
            var child = AbsolutePath.Parse(childPath);

            // Act
            var isParent = parent.IsParentDirectoryFor(child);

            // Assert
            Assert.True(isParent);
        }

        [Theory]
        [InlineData(@"C:", "C:")]
        [InlineData(@"C:\", "C:")]
        [InlineData(@"C:\temp", @"C:\temp")]
        [InlineData(@"C:\temp", "C:")]
        [InlineData(@"D:\temp", @"C:\temp\docs")]
        [InlineData(@"/etc/config", "/")]
        public void ShouldNotDetectParentDirectoryFor(string parentDirectory, string childPath)
        {
            // Arrange
            var parent = AbsolutePath.Parse(parentDirectory);
            var child = AbsolutePath.Parse(childPath);

            // Act
            var isParent = parent.IsParentDirectoryFor(child);

            // Assert
            Assert.False(isParent);
        }
    }
}
