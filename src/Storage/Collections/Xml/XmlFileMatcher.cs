using System.Threading.Tasks;
using DotNetRu.Auditor.Storage.Collections.Bindings;
using DotNetRu.Auditor.Storage.FileSystem;

namespace DotNetRu.Auditor.Storage.Collections.Xml
{
    internal sealed class XmlFileMatcher : Matcher
    {
        private readonly IXmlCollectionFactory collectionFactory;
        private int fileCount;

        public XmlFileMatcher(IXmlCollectionFactory collectionFactory)
        {
            this.collectionFactory = collectionFactory;
        }

        public override async Task AcceptAsync(IFile file)
        {
            var exists = await file.ExistsAsync().ConfigureAwait(false);
            if (!exists)
            {
                ErrorMessage = $"File {file.FullName} not found";
                return;
            }

            if (!XmlPath.HasExtension(file.Name))
            {
                ErrorMessage = $"Unknown file {file.FullName} format";
            }

            fileCount++;
        }

        public override Task AcceptAsync(IDirectory directory)
        {
            ErrorMessage = $"File based collection can't contain directory ({directory.FullName})";
            return Task.CompletedTask;
        }

        public override IDocumentCollection? Match(IDirectory collectionDirectory)
        {
            if (ErrorMessage != null)
            {
                return default;
            }

            if (fileCount == 0)
            {
                ErrorMessage = "No files detected";
                return default;
            }

            var collection = collectionFactory.Create(CollectionStructure.File, collectionDirectory);
            if (collection == null)
            {
                ErrorMessage = $"Unknown model for {collectionDirectory.Name}";
                return default;
            }

            return collection;
        }
    }
}
