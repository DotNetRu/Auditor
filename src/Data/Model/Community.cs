using System;

namespace DotNetRu.Auditor.Data.Model
{
    public sealed class Community : IDocument
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public string? City { get; set; }

        public string? TimeZone { get; set; }

        public string? VkUrl { get; set; }

        public string? TwitterUrl { get; set; }

        public string? TelegramChannelUrl { get; set; }

        public string? TelegramChatUrl { get; set; }

        public string? MeetupComUrl { get; set; }

        public string? TimePadUrl { get; set; }

        public int GetContentChecksum()
        {
            var urlChecksum = HashCode.Combine(VkUrl, TwitterUrl, TelegramChannelUrl, TelegramChatUrl, MeetupComUrl, TimePadUrl);
            return HashCode.Combine(Id, Name, City, TimeZone, urlChecksum);
        }
    }
}
