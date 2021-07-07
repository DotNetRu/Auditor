using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DotNetRu.Auditor.Storage.FileSystem;
using DotNetRu.Auditor.Storage.FileSystem.PathEngine;
using Xunit;

namespace DotNetRu.Auditor.UnitTests.Storage.FileSystem
{
    public sealed class FileSystemEntryTest
    {
        private static readonly string RootPath = AbsolutePath.Root.FullName;

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

        [Fact]
        public void ShouldResolveNameFromRoot()
        {
            // Act
            var entry = new TestEntry(RootPath);

            // Assert
            Assert.Equal(RootPath, entry.FullName);
            Assert.Equal(RootPath, entry.Name);
        }

        public static IEnumerable<string[]> GetDataForFullPathTest()
        {
            // "C:\" + "Abc" => "C:\Abc"
            yield return new[] { RootPath, "Abc", Path.Combine(RootPath, "Abc") };
            // "C:\1" + ".\Abc" => "C:\1\Abc"
            yield return new[] { Path.Combine(RootPath, "1"), Path.Combine(".", "Abc"), Path.Combine(RootPath, "1", "Abc") };
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
            var root = Path.Combine(RootPath, "A", "B");
            var subPath = Path.Combine("..", "..", "etc", "passwd");

            // Act
            void HackRoot() => GetFullPath(root, subPath);

            // Assert
            Assert.Throws<ArgumentException>(HackRoot);
        }

        private static string GetFullPath(string root, string subPath)
        {
            var entry = new TestEntry(root);
            return entry.GetFullChildName(subPath);
        }

        private sealed class TestEntry : FileSystemEntry
        {
            public TestEntry(string path)
                : base(path)
            {
            }

            public string GetFullChildName(string subPath) => base.GetFullChildPath(subPath);

            public override ValueTask<bool> ExistsAsync() => throw new NotSupportedException();
        }
    }
}
