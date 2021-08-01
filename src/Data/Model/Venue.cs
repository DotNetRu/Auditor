namespace DotNetRu.Auditor.Data.Model
{
    public sealed class Venue : IDocument
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public int? Capacity { get; set; }

        public string? Address { get; set; }

        public string? MapUrl { get; set; }
    }
}
