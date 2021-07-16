using System;
using System.Collections.Generic;
using DotNetRu.Auditor.Storage.IO;
using Xunit;

namespace DotNetRu.Auditor.UnitTests.Storage.IO
{
    public sealed class UnixNamingStrategyTest
    {
        private readonly UnixNamingStrategy strategy = UnixNamingStrategy.Instance;

        [Theory]
        [InlineData("/")]
        [InlineData("/etc/")]
        [InlineData("/etc/config")]
        public void ShouldGetRootPath(string path)
        {
            // Act
            var hasRoot = strategy.TryGetPathRoot(path, out var root);

            // Assert
            Assert.True(hasRoot);
            Assert.Equal("/", root);
        }

        [Theory]
        [InlineData("etc")]
        [InlineData("etc/config")]
        [InlineData("C:")]
        public void ShouldNotGetRootPath(string path)
        {
            // Act
            var hasRoot = strategy.TryGetPathRoot(path, out _);

            // Assert
            Assert.False(hasRoot);
        }

        [Theory]
        [InlineData("/", true)]
        [InlineData("/etc", true)]
        [InlineData("etc", false)]
        public void ShouldDetectRootedPath(string path, bool expectedIsRooted)
        {
            // Act
            var actualIsRooted = strategy.IsPathRooted(path);

            // Assert
            Assert.Equal(expectedIsRooted, actualIsRooted);
        }

        public static TheoryData<IReadOnlyList<string>, string> CombineTestCases() => new()
        {
            { Array.Empty<string>(), String.Empty },
            { new[] { "/" }, "/" },
            { new[] { "/", "etc" }, "/etc" },
            { new[] { "/", "etc", "config" }, "/etc/config" },
            { new[] { "/", "", "etc", "", "config" }, "/etc/config" }
        };

        [Theory]
        [MemberData(nameof(CombineTestCases))]
        public void ShouldCombine(IReadOnlyList<string> paths, string expectedPath)
        {
            // Act
            var actualPath = strategy.Combine(paths);

            // Assert
            Assert.Equal(expectedPath, actualPath);
        }

        [Theory]
        [InlineData("etc", true)]
        [InlineData("e T c", true)]
        [InlineData("", false)]
        [InlineData("/", false)]
        [InlineData(" \r\n\t ", false)]
        public void ShouldValidaName(string name, bool expectedValid)
        {
            // Act
            var actualValid = strategy.IsValidName(name);

            // Assert
            Assert.Equal(expectedValid, actualValid);
        }
    }
}
