using System.Collections.Generic;

namespace DotNetRu.Auditor.Data.Model
{
    public sealed class TalkRecord
    {
        public string? Id { get; set; }

        public List<string> SpeakerIds { get; } = new();

        public string? Title { get; set; }

        public string? Description { get; set; }

        public List<string> SeeAlsoTalkIds { get; } = new();

        public string? CodeUrl { get; set; }

        public string? SlidesUrl { get; set; }

        public string? VideoUrl { get; set; }
    }
}
