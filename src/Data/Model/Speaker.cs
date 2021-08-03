using System;

namespace DotNetRu.Auditor.Data.Model
{
    public sealed class Speaker : IDocument
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public string? CompanyName { get; set; }

        public string? CompanyUrl { get; set; }

        public string? Description { get; set; }

        public string? BlogUrl { get; set; }

        public string? ContactsUrl { get; set; }

        public string? TwitterUrl { get; set; }

        public string? HabrUrl { get; set; }

        public string? GitHubUrl { get; set; }

        public int GetContentChecksum()
        {
            var urlChecksum = HashCode.Combine(BlogUrl, ContactsUrl, TwitterUrl, HabrUrl, GitHubUrl);
            return HashCode.Combine(Id, Name, CompanyName, CompanyUrl, Description, urlChecksum);
        }
    }
}
