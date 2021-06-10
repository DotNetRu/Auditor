using System;
using System.IO;
using DotNetRu.Auditor.Storage.FileSystem;
using Xunit;

namespace DotNetRu.Auditor.Tests.Storage.FileSystem
{
    public sealed class FileSystemEntryTest
    {
        [Fact]
        public void ShouldResolveFullName()
        {
            // Arrange
            const string name = "Test";

            // Act
            var entry = new TestEntry(name);

            // Assert
            var fullName = entry.FullName;
            Assert.EndsWith(name, fullName);

            var hasRoot = Path.IsPathRooted(fullName);
            Assert.True(hasRoot);
        }

        [Fact]
        public void ShouldResolveName()
        {
            // Arrange
            const string expectedName = "Test";
            var fullName = Path.Combine("A", "B", expectedName);

            // Act
            var entry = new TestEntry(fullName);

            // Assert
            Assert.Equal(expectedName, entry.Name);
        }

        [Theory]
        [InlineData(@"C:\", @"Abc", @"C:\Abc")]
        [InlineData(@"C:\1", @".\Abc", @"C:\1\Abc")]
        public void ShouldResolveFullPath(string root, string subPath, string expectedFullPath)
        {
            // Act
            var fullPath = GetFullPath(root, subPath);

            // Assert
            Assert.Equal(expectedFullPath, fullPath);
        }

        [Fact]
        public void ShouldRaiseErrorWhenHackingRoot()
        {
            // Arrange
            const string root = @"C:\A\B";
            const string subPath = @"..\..\etc\passwd";

            // Act
            void HackRoot() => GetFullPath(root, subPath);

            // Assert
            Assert.Throws<ArgumentException>(HackRoot);
        }

        private static string GetFullPath(string root, string subPath)
        {
            // var root = AssertEx.NotNull(Path.GetPathRoot(typeof(FileSystemEntry).Assembly.Location));
            var entry = new TestEntry(root);
            return entry.GetFullSubPath(subPath);
        }

        private sealed class TestEntry : FileSystemEntry
        {
            public TestEntry(string fullName)
                : base(fullName, true)
            {
            }

            public string GetFullSubPath(string subPath) => base.GetFullPath(subPath);
        }
    }
}
