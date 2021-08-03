namespace DotNetRu.Auditor.Data
{
    public interface IDocument
    {
        public string? Id { get; }

        public string? Name { get; }

        public int GetContentChecksum();
    }
}
