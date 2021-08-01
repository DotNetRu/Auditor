using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotNetRu.Auditor.Data;
using DotNetRu.Auditor.Storage.Collections;
using Xunit;

namespace DotNetRu.Auditor.UnitTests.Storage.Collections
{
    public abstract class CollectionTest
    {
        protected const string KnownId = "passwd";

        private readonly Collection collection;

        internal CollectionTest(Collection collection)
        {
            this.collection = collection;
        }

        [Fact]
        public void ShouldHaveName()
        {
            // Act
            var collectionName = collection.Name;

            // Assert
            Assert.NotEmpty(collectionName);
        }

        [Fact]
        public async Task ShouldLoadSingle()
        {
            // Act
            var secret = await collection.LoadAsync(KnownId, DeserializeSecret).ConfigureAwait(false);

            // Assert
            Assert.NotNull(secret);
            Assert.Equal(KnownId, secret?.Id);
        }

        [Fact]
        public async Task ShouldReturnDefaultWhenLoadNotExistingSingle()
        {
            // Arrange
            const string unknownId = "shadow";

            // Act
            var secret = await collection.LoadAsync(unknownId, DeserializeSecret).ConfigureAwait(false);

            // Assert
            Assert.Null(secret);
        }

        [Fact]
        public async Task ShouldLoadMany()
        {
            // Act
            var secretList = await collection.QueryAsync(DeserializeSecret).ToListAsync().ConfigureAwait(false);

            // Assert
            Assert.True(secretList.Count > 1);
            var secrets = AssertEx.ItemNotNull(secretList);
            Assert.All(secrets, secret => Assert.StartsWith(KnownId, AssertEx.NotNull(secret.Id)));
        }

        [Fact]
        public async Task ShouldSkipWhenCantDeserialize()
        {
            // Arrange
            var secretCount = 0;
            Task<Secret?> DeserializeFirstSecret(Stream stream) =>
                ++secretCount == 1 ? DeserializeSecret(stream) : Task.FromResult<Secret?>(null);

            // Act
            var secretList = await collection.QueryAsync(DeserializeFirstSecret).ToListAsync().ConfigureAwait(false);

            // Assert
            var singleSecret = Assert.Single(secretList);
            var secret = AssertEx.NotNull(singleSecret);
            Assert.StartsWith(KnownId, AssertEx.NotNull(secret.Id));
        }

        private static async Task<Secret?> DeserializeSecret(Stream stream)
        {
            var state = await stream.ReadAllTextAsync().ConfigureAwait(false);
            return new Secret(state);
        }

        private sealed record Secret(string? Id) : IDocument
        {
            public string? Name => Id;
        }
    }
}
