using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetRu.Auditor.Data;
using DotNetRu.Auditor.Storage.FileSystem;

namespace DotNetRu.Auditor.Storage.Collections.Xml
{
    internal sealed class XmlDirectoryCollection<T> : Collection<T>
        where T : IDocument
    {
        public XmlDirectoryCollection(IDirectory directory, IDocumentSerializer<T> serializer)
            : base(directory, serializer)
        {
        }

        protected override IFile GetIndexFileAsync(string id)
        {
            var indexDirectory = Directory.GetDirectory(id);
            var indexFile = indexDirectory.GetFile(XmlPath.IndexFileName);
            return indexFile;
        }

        protected override async IAsyncEnumerable<IFile> EnumerateIndexFilesAsync()
        {
            await foreach (var indexDirectory in Directory.EnumerateDirectoriesAsync().ConfigureAwait(false))
            {
                yield return indexDirectory.GetFile(XmlPath.IndexFileName);
            }
        }
    }
}
