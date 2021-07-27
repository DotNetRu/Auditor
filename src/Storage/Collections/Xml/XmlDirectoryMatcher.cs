using System.Threading.Tasks;
using DotNetRu.Auditor.Storage.Collections.Bindings;
using DotNetRu.Auditor.Storage.FileSystem;

namespace DotNetRu.Auditor.Storage.Collections.Xml
{
    internal sealed class XmlDirectoryMatcher : Matcher
    {
        private readonly IDirectory collectionDirectory;
        private int directoryCount;

        public XmlDirectoryMatcher(IDirectory collectionDirectory)
        {
            this.collectionDirectory = collectionDirectory;
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

        public override Collection? Match()
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

            return new XmlDirectoryCollection(collectionDirectory);
        }
    }
}
