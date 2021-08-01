using System.Collections.Generic;

namespace DotNetRu.Auditor.Data.Model
{
    public sealed class Talk : IDocument
    {
        public string? Id { get; set; }

        public List<string> SpeakerIds { get; } = new();

        public string? Name { get; set; }

        public string? Description { get; set; }

        public List<string> SeeAlsoTalkIds { get; } = new();

        public string? CodeUrl { get; set; }

        public string? SlidesUrl { get; set; }

        public string? VideoUrl { get; set; }
    }
}
