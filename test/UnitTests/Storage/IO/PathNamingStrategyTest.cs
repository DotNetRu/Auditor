using DotNetRu.Auditor.Storage.IO;
using Xunit;

namespace DotNetRu.Auditor.UnitTests.Storage.IO
{
    public sealed class PathNamingStrategyTest
    {
        [Fact]
        public void ShouldResolveWindowsStrategy()
        {
            // Arrange
            const string path = @"C:\temp";

            // Act
            var canResolve = PathNamingStrategy.TryResolve(path, out var nameStrategy);

            // Assert
            Assert.True(canResolve);
            Assert.IsType<WindowsNamingStrategy>(nameStrategy);
        }

        [Fact]
        public void ShouldResolveUnixStrategy()
        {
            // Arrange
            const string path = @"/tmp";

            // Act
            var canResolve = PathNamingStrategy.TryResolve(path, out var nameStrategy);

            // Assert
            Assert.True(canResolve);
            Assert.IsType<UnixNamingStrategy>(nameStrategy);
        }

        [Fact]
        public void ShouldNotResolveStrategyByRelativePath()
        {
            // Arrange
            const string path = @"root";

            // Act
            var canResolve = PathNamingStrategy.TryResolve(path, out var nameStrategy);

            // Assert
            Assert.False(canResolve);
            Assert.Null(nameStrategy);
        }

        [Theory]
        [InlineData(@"A\B\\C")]
        [InlineData(@"/A//B/C///")]
        public void ShouldSplit(string path)
        {
            // Act
            var parts = PathNamingStrategy.Split(path);

            // Assert
            Assert.Equal(3, parts.Count);
            Assert.Equal("A", parts[0]);
            Assert.Equal("B", parts[1]);
            Assert.Equal("C", parts[2]);
        }
    }
}
