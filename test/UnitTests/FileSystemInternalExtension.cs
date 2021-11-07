using System;
using System.IO;
using System.Threading.Tasks;
using DotNetRu.Auditor.Storage.Collections.Xml;
using DotNetRu.Auditor.Storage.FileSystem;

namespace DotNetRu.Auditor.UnitTests
{
    internal static class FileSystemInternalExtension
    {
        public static Task WriteToXmlDirectoryCollectionAsync(this IDirectory collectionDirectory, string id, string content = "") => collectionDirectory
            .GetDirectory(id)
            .GetFile(XmlPath.IndexFileName)
            .WriteAllTextAsync(content);

        public static Task WriteToXmlFileCollectionAsync(this IDirectory collectionDirectory, string id, string content = "") => collectionDirectory
            .GetFile(XmlPath.ChangeExtension(id))
            .WriteAllTextAsync(content);
    }
}
