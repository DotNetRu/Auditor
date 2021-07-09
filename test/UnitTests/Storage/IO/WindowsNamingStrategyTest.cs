using System;
using System.Collections.Generic;
using DotNetRu.Auditor.Storage.IO;
using Xunit;

namespace DotNetRu.Auditor.UnitTests.Storage.IO
{
    public sealed class WindowsNamingStrategyTest
    {
        private readonly WindowsNamingStrategy strategy = WindowsNamingStrategy.Instance;

        [Theory]
        [InlineData(@"C:")]
        [InlineData(@"C:\temp")]
        [InlineData(@"C:/temp/docs")]
        public void ShouldGetRootPath(string path)
        {
            // Act
            var hasRoot = strategy.TryGetPathRoot(path, out var root);

            // Assert
            Assert.True(hasRoot);
            Assert.Equal(@"C:\", root);
        }

        [Theory]
        [InlineData("temp")]
        [InlineData(@"Data:\")]
        [InlineData("C")]
        [InlineData("/")]
        public void ShouldNotGetRootPath(string path)
        {
            // Act
            var hasRoot = strategy.TryGetPathRoot(path, out _);

            // Assert
            Assert.False(hasRoot);
        }

        [Theory]
        [InlineData(@"C:", true)]
        [InlineData(@"C:/temp", true)]
        [InlineData("temp", false)]
        public void ShouldDetectRootedPath(string path, bool expectedIsRooted)
        {
            // Act
            var actualIsRooted = strategy.IsPathRooted(path);

            // Assert
            Assert.Equal(expectedIsRooted, actualIsRooted);
        }

        public static IEnumerable<object[]> CombineTestCases()
        {
            yield return Case(String.Empty);
            yield return Case(@"C:", "C:");
            yield return Case(@"C:\temp", "C:", "temp");
            yield return Case(@"C:\temp", @"C:\", "temp");
            yield return Case(@"C:\temp\docs", @"C:\", "temp", "docs");
            yield return Case(@"C:\temp\docs", @"C:\", "", "temp", "", "docs");

            static object[] Case(string expectedPath, params string[] paths) => new object[] { paths, expectedPath };
        }

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
        [InlineData("temp", true)]
        [InlineData("T e M p", true)]
        [InlineData("", false)]
        [InlineData("*.txt", false)]
        [InlineData("life > null", false)]
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