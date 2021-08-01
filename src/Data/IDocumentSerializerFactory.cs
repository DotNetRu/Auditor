namespace DotNetRu.Auditor.Data
{
    public interface IDocumentSerializerFactory
    {
        IDocumentSerializer<T> Create<T>()
            where T : IDocument;
    }
}
