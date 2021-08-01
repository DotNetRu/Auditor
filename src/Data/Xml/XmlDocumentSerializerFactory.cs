using System;
using System.Collections.Concurrent;
using DotNetRu.Auditor.Data.Model;

using IEntitySerializer = System.Object;

namespace DotNetRu.Auditor.Data.Xml
{
    public sealed class XmlDocumentSerializerFactory : IDocumentSerializerFactory
    {
        private static readonly ConcurrentDictionary<Type, IEntitySerializer> Cache = new();

        public IDocumentSerializer<T> Create<T>()
            where T : IDocument
        {
            // NOTE: When we use XmlSerializer with overrides, framework cannot reuse generated assemblies.
            // Therefore, to avoid memory leaks, we should cache all serializers with the same overrides.
            var serializer = Cache.GetOrAdd(typeof(T), _ => Build<T>());
            return (IDocumentSerializer<T>)serializer;
        }

        internal static XmlModelBuilder<T> CreateModelBuilder<T>()
            where T : IDocument
        {
            object builder = typeof(T) switch
            {
                Type type when type == typeof(Community) => BuildCommunity(),
                Type type when type == typeof(Meetup) => BuildMeetup(),
                Type type when type == typeof(Speaker) => BuildSpeaker(),
                Type type when type == typeof(Talk) => BuildTalk(),
                Type type when type == typeof(Venue) => BuildVenue(),
                Type type when type == typeof(Friend) => BuildFriend(),

                _ => throw new InvalidOperationException($"Unknown model type: {typeof(T).FullName}")
            };

            return (XmlModelBuilder<T>)builder;
        }

        private static IDocumentSerializer<T> Build<T>()
            where T : IDocument
        {
            var overrides = CreateModelBuilder<T>().Build();
            return new XmlDocumentSerializer<T>(overrides);
        }

        private static XmlModelBuilder<Community> BuildCommunity() => XmlModelBuilder<Community>
            .Map("Community")
            .Property(community => community.Id, "Id")
            .Property(community => community.Name, "Name")
            .Property(community => community.City, "City")
            .Property(community => community.TimeZone, "TimeZone")
            .Property(community => community.VkUrl, "VkUrl")
            .Property(community => community.TwitterUrl, "TwitterUrl")
            .Property(community => community.TelegramChannelUrl, "TelegramChannelUrl")
            .Property(community => community.TelegramChatUrl, "TelegramChatUrl")
            .Property(community => community.MeetupComUrl, "MeetupComUrl")
            .Property(community => community.TimePadUrl, "TimePadUrl");

        private static XmlModelBuilder<Meetup> BuildMeetup() => XmlModelBuilder<Meetup>
            .Map("Meetup")
            .Property(meetup => meetup.Id, "Id")
            .Property(meetup => meetup.Name, "Name")
            .Property(meetup => meetup.CommunityId, "CommunityId")
            .Collection(meetup => meetup.FriendIds, "FriendIds", "FriendId")
            .Property(meetup => meetup.VenueId, "VenueId")
            .Collection(meetup => meetup.Sessions, "Sessions", "Session", sessionMap =>
            {
                sessionMap
                    .Property(session => session.TalkId, "TalkId")
                    .Property(session => session.StartTime, "StartTime")
                    .Property(session => session.EndTime, "EndTime");
            });

        private static XmlModelBuilder<Speaker> BuildSpeaker() => XmlModelBuilder<Speaker>
            .Map("Speaker")
            .Property(speaker => speaker.Id, "Id")
            .Property(speaker => speaker.Name, "Name")
            .Property(speaker => speaker.CompanyName, "CompanyName")
            .Property(speaker => speaker.CompanyUrl, "CompanyUrl")
            .Property(speaker => speaker.Description, "Description")
            .Property(speaker => speaker.BlogUrl, "BlogUrl")
            .Property(speaker => speaker.ContactsUrl, "ContactsUrl")
            .Property(speaker => speaker.TwitterUrl, "TwitterUrl")
            .Property(speaker => speaker.HabrUrl, "HabrUrl")
            .Property(speaker => speaker.GitHubUrl, "GitHubUrl");

        private static XmlModelBuilder<Talk> BuildTalk() => XmlModelBuilder<Talk>
            .Map("Talk")
            .Property(talk => talk.Id, "Id")
            .Collection(talk => talk.SpeakerIds, "SpeakerIds", "SpeakerId")
            .Property(talk => talk.Name, "Title")
            .Property(talk => talk.Description, "Description")
            .Collection(talk => talk.SeeAlsoTalkIds, "SeeAlsoTalkIds", "TalkId")
            .Property(talk => talk.CodeUrl, "CodeUrl")
            .Property(talk => talk.SlidesUrl, "SlidesUrl")
            .Property(talk => talk.VideoUrl, "VideoUrl");

        private static XmlModelBuilder<Venue> BuildVenue() => XmlModelBuilder<Venue>
            .Map("Venue")
            .Property(venue => venue.Id, "Id")
            .Property(venue => venue.Name, "Name")
            .Property(venue => venue.Capacity, "Capacity")
            .Property(venue => venue.Address, "Address")
            .Property(venue => venue.MapUrl, "MapUrl");

        private static XmlModelBuilder<Friend> BuildFriend() => XmlModelBuilder<Friend>
            .Map("Friend")
            .Property(friend => friend.Id, "Id")
            .Property(friend => friend.Name, "Name")
            .Property(friend => friend.Url, "Url")
            .Property(friend => friend.Description, "Description");
    }
}
