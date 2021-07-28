using System;

namespace DotNetRu.Auditor.Data.Model
{
    public sealed class MeetupSession
    {
        public string? TalkId { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }
    }
}
