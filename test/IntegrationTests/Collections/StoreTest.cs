using System.Linq;
using System.Threading.Tasks;
using DotNetRu.Auditor.Data.Model;
using DotNetRu.Auditor.Storage;
using Xunit;

namespace DotNetRu.Auditor.IntegrationTests.Collections
{
    // TDO: More fully integration tests for Session after completion of Modification feature
    [Collection(StoreFixture.Name)]
    public sealed class StoreTest
    {
        private readonly ISession session;

        public StoreTest(StoreFixture fixture)
        {
            session = fixture.Storage.OpenSession();
        }

        [Fact]
        public async Task ShouldReadAllCommunities()
        {
            // Act
            var communities = await session.QueryAsync<Community>().ToListAsync().ConfigureAwait(false);

            // Assert
            var singleCommunity = Assert.Single(communities);
            var community = AssertEx.NotNull(singleCommunity);

            Assert.Equal("AllDotNet", community.Id);
            Assert.Equal("AllDotNet Community", community.Name);
        }
    }
}
