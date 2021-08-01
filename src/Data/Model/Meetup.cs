using System.Collections.Generic;

namespace DotNetRu.Auditor.Data.Model
{
    public sealed class Meetup : IDocument
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public string? CommunityId { get; set; }

        public List<string> FriendIds { get; } = new();

        public string? VenueId { get; set; }

        public List<MeetupSession> Sessions { get; } = new();
    }
}
