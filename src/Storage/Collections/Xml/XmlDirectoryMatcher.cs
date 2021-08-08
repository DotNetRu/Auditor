using System.Threading.Tasks;
using DotNetRu.Auditor.Storage.Collections.Bindings;
using DotNetRu.Auditor.Storage.FileSystem;

namespace DotNetRu.Auditor.Storage.Collections.Xml
{
    internal sealed class XmlDirectoryMatcher : Matcher
    {
        private readonly IXmlCollectionFactory collectionFactory;
        private int directoryCount;

        public XmlDirectoryMatcher(IXmlCollectionFactory collectionFactory)
        {
            this.collectionFactory = collectionFactory;
        }

        public override Task AcceptAsync(IFile file)
        {
            ErrorMessage = $"Directory based collection can't contain files ({file.FullName})";
            return Task.CompletedTask;
        }

        public override async Task AcceptAsync(IDirectory directory)
        {
            var indexFile = directory.GetFile(XmlPath.IndexFileName);
            var exists = await indexFile.ExistsAsync().ConfigureAwait(false);
            if (!exists)
            {
                ErrorMessage = $"File {XmlPath.IndexFileName} not found in {directory.FullName}";
                return;
            }

            directoryCount++;
        }

        public override IDocumentCollection? Match(IDirectory collectionDirectory)
        {
            if (ErrorMessage != null)
            {
                return default;
            }

            if (directoryCount == 0)
            {
                ErrorMessage = "No directories detected";
                return default;
            }

            var collection = collectionFactory.Create(CollectionStructure.Directory, collectionDirectory);
            if (collection == null)
            {
                ErrorMessage = $"Unknown model for {collectionDirectory.Name}";
                return default;
            }

            return collection;
        }
    }
}
