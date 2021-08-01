using System.Threading.Tasks;
using DotNetRu.Auditor.Data;
using DotNetRu.Auditor.Data.Model;
using DotNetRu.Auditor.Data.Xml;
using DotNetRu.Auditor.Storage;
using DotNetRu.Auditor.Storage.FileSystem;
using DotNetRu.Auditor.Storage.IO;
using Xunit;

namespace DotNetRu.Auditor.IntegrationTests.Collections
{
    [CollectionDefinition(Name)]
    public sealed class StoreFixture : ICollectionFixture<StoreFixture>
    {
        public const string Name = nameof(StoreFixture);

        public StoreFixture()
        {
            var memory = MemoryFileSystem.ForDirectory(AbsolutePath.Root.Child("memory").FullName);

            Storage = InitializeAsync(memory).GetAwaiter().GetResult();
        }

        public IStore Storage { get; }

        private static async Task<IStore> InitializeAsync(IDirectory root)
        {
            var community = new Community
            {
                Id = "AllDotNet",
                Name = "AllDotNet Community"
            };

            var serializerFactory = new XmlDocumentSerializerFactory();
            var serializer = serializerFactory.Create<Community>();

            await CreateFileAsync(root, community, serializer).ConfigureAwait(false);

            var store = await AuditStore.OpenAsync(root).ConfigureAwait(false);
            return store;
        }

        private static async Task CreateFileAsync(
            IDirectory root,
            Community community,
            IDocumentSerializer<Community> serializer)
        {
            if (community.Id == null)
            {
                return;
            }

            var file = root.GetDirectory("communities").GetFile(community.Id + ".xml");
            var fileExists = await file.ExistsAsync().ConfigureAwait(false);
            Assert.False(fileExists);

            var writableFile = await file.RequestWriteAccessAsync().ConfigureAwait(false);
            writableFile = AssertEx.NotNull(writableFile);

            var fileStream = await writableFile.OpenForWriteAsync().ConfigureAwait(false);
            await using (fileStream.ConfigureAwait(false))
            {
                await serializer.SerializeAsync(fileStream, community).ConfigureAwait(false);
            }

            fileExists = await file.ExistsAsync().ConfigureAwait(false);
            Assert.True(fileExists);
        }
    }
}
