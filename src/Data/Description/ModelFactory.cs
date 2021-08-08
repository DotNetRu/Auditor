using System;
using System.Collections.Generic;
using DotNetRu.Auditor.Data.Model;
using DotNetRu.Auditor.Data.Xml;

namespace DotNetRu.Auditor.Data.Description
{
    internal static class ModelFactory
    {
        public static IReadOnlyList<ModelDefinition> CreateModels()
        {
            var models = new[]
            {
                Define(Community()),
                Define(Meetup()),
                Define(Speaker()),
                Define(Talk()),
                Define(Venue()),
                Define(Friend())
            };

            return models;
        }

        private static ModelDefinition Define<T>(XmlModelBuilder<T> builder)
            where T : IDocument
        {
            if (builder.Name == null || builder.GroupName == null)
            {
                throw new InvalidOperationException("Model builder can't have empty Name or Group Name");
            }

            // NOTE: When we use XmlSerializer with overrides, framework cannot reuse generated assemblies.
            // Therefore, to avoid memory leaks, we should cache all serializers with the same overrides.
            var serializer = new XmlDocumentSerializer<T>(builder.Overrides);

            return new ModelDefinition(builder.Name, builder.GroupName, typeof(T), serializer);
        }

        private static XmlModelBuilder<Community> Community() => XmlModelBuilder<Community>
            .Map("Community", "Communities")
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

        private static XmlModelBuilder<Meetup> Meetup() => XmlModelBuilder<Meetup>
            .Map("Meetup", "Meetups")
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

        private static XmlModelBuilder<Speaker> Speaker() => XmlModelBuilder<Speaker>
            .Map("Speaker", "Speakers")
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

        private static XmlModelBuilder<Talk> Talk() => XmlModelBuilder<Talk>
            .Map("Talk", "Talks")
            .Property(talk => talk.Id, "Id")
            .Collection(talk => talk.SpeakerIds, "SpeakerIds", "SpeakerId")
            .Property(talk => talk.Name, "Title")
            .Property(talk => talk.Description, "Description")
            .Collection(talk => talk.SeeAlsoTalkIds, "SeeAlsoTalkIds", "TalkId")
            .Property(talk => talk.CodeUrl, "CodeUrl")
            .Property(talk => talk.SlidesUrl, "SlidesUrl")
            .Property(talk => talk.VideoUrl, "VideoUrl");

        private static XmlModelBuilder<Venue> Venue() => XmlModelBuilder<Venue>
            .Map("Venue", "Venues")
            .Property(venue => venue.Id, "Id")
            .Property(venue => venue.Name, "Name")
            .Property(venue => venue.Capacity, "Capacity")
            .Property(venue => venue.Address, "Address")
            .Property(venue => venue.MapUrl, "MapUrl");

        private static XmlModelBuilder<Friend> Friend() => XmlModelBuilder<Friend>
            .Map("Friend", "Friends")
            .Property(friend => friend.Id, "Id")
            .Property(friend => friend.Name, "Name")
            .Property(friend => friend.Url, "Url")
            .Property(friend => friend.Description, "Description");
    }
}
