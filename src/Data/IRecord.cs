namespace DotNetRu.Auditor.Data
{
    // TDO: Remove «Record» suffix from inheritors
    public interface IRecord
    {
        public string? Id { get; }

        public string? Name { get; }
    }
}
