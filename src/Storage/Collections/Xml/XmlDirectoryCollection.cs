using System.Collections.Generic;
using DotNetRu.Auditor.Storage.FileSystem;

namespace DotNetRu.Auditor.Storage.Collections.Xml
{
    internal sealed class XmlDirectoryCollection : Collection
    {
        public XmlDirectoryCollection(IDirectory directory)
            : base(directory)
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
            await foreach (var indexDirectory in Directory.EnumerateDirectoriesAsync())
            {
                yield return indexDirectory.GetFile(XmlPath.IndexFileName);
            }
        }
    }
}
