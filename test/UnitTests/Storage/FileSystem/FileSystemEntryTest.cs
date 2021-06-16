using System;
using System.Collections.Generic;
using System.IO;
using DotNetRu.Auditor.Storage.FileSystem;
using Xunit;

namespace DotNetRu.Auditor.Tests.Storage.FileSystem
{
    public sealed class FileSystemEntryTest
    {
        private static readonly string PathRoot = Path.GetPathRoot(Directory.GetCurrentDirectory()) ?? String.Empty;

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

        public static IEnumerable<object[]> GetDataForFullPathTest()
        {
            // "C:\" + "Abc" => "C:\Abc"
            yield return new object[] { PathRoot, "Abc", Path.Combine(PathRoot, "Abc") };
            // "C:\1" + ".\Abc" => "C:\1\Abc"
            yield return new object[] { Path.Combine(PathRoot, "1"), Path.Combine(".", "Abc"), Path.Combine(PathRoot, "1", "Abc") };
        }

        [Theory]
        [MemberData(nameof(GetDataForFullPathTest))]
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
            var root = Path.Combine(PathRoot, "A", "B");
            var subPath = Path.Combine("..", "..", "etc", "passwd");

            // Act
            void HackRoot() => GetFullPath(root, subPath);

            // Assert
            Assert.Throws<ArgumentException>(HackRoot);
        }

        private static string GetFullPath(string root, string subPath)
        {
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
