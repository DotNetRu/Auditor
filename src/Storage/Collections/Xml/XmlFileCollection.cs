using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetRu.Auditor.Storage.FileSystem;

namespace DotNetRu.Auditor.Storage.Collections.Xml
{
    internal sealed class XmlFileCollection : Collection
    {
        public XmlFileCollection(IDirectory directory)
            : base(directory)
        {
        }

        protected override IFile GetIndexFileAsync(string id)
        {
            var indexFileName = XmlPath.ChangeExtension(id);
            var indexFile = Directory.GetFile(indexFileName);
            return indexFile;
        }

        protected override async IAsyncEnumerable<IFile> EnumerateIndexFilesAsync()
        {
            await foreach (var indexFile in Directory.EnumerateFilesAsync().ConfigureAwait(false))
            {
                yield return indexFile;
            }
        }
    }
}
