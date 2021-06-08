using System;
using System.Collections.Concurrent;
using System.Xml.Serialization;
using DotNetRu.Auditor.Data.Model;

using IEntitySerializer = System.Object;

namespace DotNetRu.Auditor.Data.Xml
{
    internal sealed class XmlDataSerializerFactory : IDataSerializerFactory
    {
        private static readonly ConcurrentDictionary<Type, IEntitySerializer> Cache = new();

        public IDataSerializer<T> Create<T>()
            where T : class
        {
            // NOTE: When we use XmlSerializer with overrides, framework cannot reuse generated assemblies.
            // Therefore, to avoid memory leaks, we should cache all serializers with the same overrides.
            var serializer = Cache.GetOrAdd(typeof(T), _ => Build<T>());
            return (IDataSerializer<T>)serializer;
        }

        internal static XmlModelBuilder<T> CreateModelBuilder<T>()
        {
            object builder = typeof(T) switch
            {
                Type type when type == typeof(CommunityRecord) => BuildCommunity(),
                Type type when type == typeof(MeetupRecord) => BuildMeetup(),
                Type type when type == typeof(SpeakerRecord) => BuildSpeaker(),
                Type type when type == typeof(TalkRecord) => BuildTalk(),
                Type type when type == typeof(VenueRecord) => BuildVenue(),
                Type type when type == typeof(FriendRecord) => BuildFriend(),

                _ => throw new InvalidOperationException($"Unknown model type: {typeof(T).FullName}")
            };

            return (XmlModelBuilder<T>) builder;
        }

        private static IDataSerializer<T> Build<T>()
            where T : class
        {
            var overrides = CreateModelBuilder<T>().Build();
            return new XmlDataSerializer<T>(overrides);
        }

        private static XmlModelBuilder<CommunityRecord> BuildCommunity() => XmlModelBuilder<CommunityRecord>
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

        private static XmlModelBuilder<MeetupRecord> BuildMeetup() => XmlModelBuilder<MeetupRecord>
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

        private static XmlModelBuilder<SpeakerRecord> BuildSpeaker() => XmlModelBuilder<SpeakerRecord>
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

        private static XmlModelBuilder<TalkRecord> BuildTalk() => XmlModelBuilder<TalkRecord>
            .Map("Talk")
            .Property(talk => talk.Id, "Id")
            .Collection(talk => talk.SpeakerIds, "SpeakerIds", "SpeakerId")
            .Property(talk => talk.Title, "Title")
            .Property(talk => talk.Description, "Description")
            .Collection(talk => talk.SeeAlsoTalkIds, "SeeAlsoTalkIds", "TalkId")
            .Property(talk => talk.CodeUrl, "CodeUrl")
            .Property(talk => talk.SlidesUrl, "SlidesUrl")
            .Property(talk => talk.VideoUrl, "VideoUrl");

        private static XmlModelBuilder<VenueRecord> BuildVenue() => XmlModelBuilder<VenueRecord>
            .Map("Venue")
            .Property(venue => venue.Id, "Id")
            .Property(venue => venue.Name, "Name")
            .Property(venue => venue.Capacity, "Capacity")
            .Property(venue => venue.Address, "Address")
            .Property(venue => venue.MapUrl, "MapUrl");

        private static XmlModelBuilder<FriendRecord> BuildFriend() => XmlModelBuilder<FriendRecord>
            .Map("Friend")
            .Property(friend => friend.Id, "Id")
            .Property(friend => friend.Name, "Name")
            .Property(friend => friend.Url, "Url")
            .Property(friend => friend.Description, "Description");
    }
}
