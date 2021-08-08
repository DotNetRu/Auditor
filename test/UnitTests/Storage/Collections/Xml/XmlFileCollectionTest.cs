using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotNetRu.Auditor.Data;
using DotNetRu.Auditor.Data.Model;
using DotNetRu.Auditor.Storage.Collections;
using DotNetRu.Auditor.Storage.Collections.Xml;
using DotNetRu.Auditor.Storage.FileSystem;
using DotNetRu.Auditor.Storage.IO;
using Moq;

namespace DotNetRu.Auditor.UnitTests.Storage.Collections.Xml
{
    public sealed class XmlFileCollectionTest : CollectionTest
    {
        public XmlFileCollectionTest()
            : base(Initialize())
        {
        }

        internal override Collection<Community> CreateCollectionWithBadSerializer()
        {
            var callCount = 0;
            var realSerializer = CreateSerializer();
            Task<Community?> DeserializeFirstDocument(Stream stream) =>
                ++callCount == 1 ? realSerializer.DeserializeAsync(stream) : Task.FromResult(default(Community?));

            var serializer = new Mock<IDocumentSerializer<Community>>(MockBehavior.Strict);
            serializer.Setup(s => s.DeserializeAsync(It.IsAny<Stream>())).Returns<Stream>(DeserializeFirstDocument);

            var directory = FillFiles().GetAwaiter().GetResult();

            return new XmlFileCollection<Community>(directory, serializer.Object);
        }

        private static XmlFileCollection<Community> Initialize() => InitializeAsync().GetAwaiter().GetResult();

        private static async Task<XmlFileCollection<Community>> InitializeAsync()
        {
            var directory = await FillFiles().ConfigureAwait(false);
            var serializer = CreateSerializer();
            return new XmlFileCollection<Community>(directory, serializer);
        }

        private static async Task<IDirectory> FillFiles()
        {
            var root = MemoryFileSystem.ForDirectory(AbsolutePath.Root.FullName);

            await new[]
                {
                    KnownId,
                    KnownId + "-2"
                }
                .Select(id => root.WriteToXmlFileCollectionAsync(id, Mocker.CommunityState(id)))
                .WhenAll()
                .ConfigureAwait(false);

            return root;
        }
    }
}
