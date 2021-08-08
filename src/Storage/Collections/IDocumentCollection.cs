using System;

namespace DotNetRu.Auditor.Storage.Collections
{
    internal interface IDocumentCollection
    {
        string Name { get; }

        Type CollectionType { get; }
    }
}
