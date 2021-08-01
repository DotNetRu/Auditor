using System.Linq;
using System.Threading.Tasks;
using DotNetRu.Auditor.Data;
using DotNetRu.Auditor.Storage.Collections.Xml;
using DotNetRu.Auditor.Storage.FileSystem;
using DotNetRu.Auditor.Storage.IO;

namespace DotNetRu.Auditor.UnitTests.Storage.Collections.Xml
{
    public sealed class XmlFileCollectionTest : CollectionTest
    {
        public XmlFileCollectionTest()
            : base(Initialize())
        {
        }

        private static XmlFileCollection Initialize() => InitializeAsync().GetAwaiter().GetResult();

        private static async Task<XmlFileCollection> InitializeAsync()
        {
            var root = MemoryFileSystem.ForDirectory(AbsolutePath.Root.FullName);

            await new[]
                {
                    KnownId,
                    KnownId + "-2"
                }
                .Select(id => root.WriteToXmlFileCollectionAsync(id, KnownId))
                .WhenAll()
                .ConfigureAwait(false);

            return new XmlFileCollection(root);
        }
    }
}
