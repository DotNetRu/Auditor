using System.IO;
using System.Threading.Tasks;
using DotNetRu.Auditor.Storage.FileSystem.Memory;
using Xunit;

namespace DotNetRu.Auditor.UnitTests.Storage.FileSystem.Memory
{
    public sealed class ReusableStreamTest
    {
        [Fact]
        public async Task ShouldBeReusable()
        {
            // Arrange
            const string data = "hello";
            var stream = new ReusableStream();

            await using (var writer = new StreamWriter(stream))
            {
                await writer.WriteAsync(data).ConfigureAwait(false);
            }

            using var reader = new StreamReader(stream);

            // Act
            for (var i = 0; i < 5; i++)
            {
                stream.Seek(0, SeekOrigin.Begin);
                var line = await reader.ReadLineAsync().ConfigureAwait(false);

                // Assert
                Assert.Equal(data, line);

                stream.Close();
                await stream.DisposeAsync();
            }

            stream.RealClose();
        }
    }
}
