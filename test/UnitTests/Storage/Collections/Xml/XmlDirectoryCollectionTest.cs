using System.Linq;
using System.Threading.Tasks;
using DotNetRu.Auditor.Data;
using DotNetRu.Auditor.Storage.Collections.Xml;
using DotNetRu.Auditor.Storage.FileSystem;
using DotNetRu.Auditor.Storage.IO;

namespace DotNetRu.Auditor.UnitTests.Storage.Collections.Xml
{
    public sealed class XmlDirectoryCollectionTest : CollectionTest
    {
        public XmlDirectoryCollectionTest()
            : base(Initialize())
        {
        }

        private static XmlDirectoryCollection Initialize() => InitializeAsync().GetAwaiter().GetResult();

        private static async Task<XmlDirectoryCollection> InitializeAsync()
        {
            var root = MemoryFileSystem.ForDirectory(AbsolutePath.Root.FullName);

            await new[]
                {
                    KnownId,
                    KnownId + "-2"
                }
                .Select(id => root.WriteToXmlDirectoryCollectionAsync(id, KnownId))
                .WhenAll()
                .ConfigureAwait(false);

            return new XmlDirectoryCollection(root);
        }
    }
}
