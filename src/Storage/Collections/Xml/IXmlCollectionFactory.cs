using DotNetRu.Auditor.Storage.FileSystem;

namespace DotNetRu.Auditor.Storage.Collections.Xml
{
    internal interface IXmlCollectionFactory
    {
        IDocumentCollection? Create(CollectionStructure structure, IDirectory collectionDirectory);
    }
}
