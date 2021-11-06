using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetRu.Auditor.Data;
using DotNetRu.Auditor.Storage.FileSystem;

namespace DotNetRu.Auditor.Storage.Collections.Xml
{
    internal sealed class XmlFileCollection<T> : Collection<T>
        where T : IDocument
    {
        public XmlFileCollection(IDirectory directory, IDocumentSerializer<T> serializer)
            : base(directory, serializer)
        {
        }


        protected override IFile GetIndexFile(string id)
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
