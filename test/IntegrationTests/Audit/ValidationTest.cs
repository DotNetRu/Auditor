using System.Threading.Tasks;
using DotNetRu.Auditor.Data.Model;
using DotNetRu.Auditor.Storage;
using Xunit;
using System.Linq;

namespace DotNetRu.Auditor.IntegrationTests.Audit
{
    [Collection(AuditFixture.Name)]
    public sealed class ValidationTest
    {
        // If we find Audit directory in the local file system, we will run all the tests.
        // Otherwise, we will skip all the tests.
        // TODO: Move all this tests to GitHub file system and make the Session mandatory
        private readonly IReadOnlySession? session;

        public ValidationTest(AuditFixture fixture)
        {
            // TODO: Move all validations to Validation Domain
            session = fixture.Store?.OpenSession();
        }

        [Fact]
        public async Task ShouldHaveValidCommunities()
        {
            // Arrange
            if (session == null)
            {
                return;
            }

            // Act
            var communities = await session.QueryAsync<Community>().ToListAsync().ConfigureAwait(false);

            // Assert
            Assert.NotEmpty(communities);

            foreach (var community in communities)
            {
                Assert.NotNull(community.Id);
                Assert.NotNull(community.Name);
                Assert.NotNull(community.City);
                Assert.NotNull(community.TimeZone);
            }
        }

        [Fact]
        public async Task ShouldHaveValidMeetups()
        {
            // Arrange
            if (session == null)
            {
                return;
            }

            // Act
            var meetups = await session.QueryAsync<Meetup>().ToListAsync().ConfigureAwait(false);

            // Assert
            Assert.NotEmpty(meetups);

            foreach (var meetup in meetups)
            {
                Assert.NotNull(meetup.Id);
                Assert.NotNull(meetup.Name);
                Assert.NotNull(meetup.CommunityId);
                Assert.NotEmpty(meetup.Sessions);
                // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
                Assert.All(meetup.Sessions, meetupSession =>
                {
                    Assert.NotNull(meetupSession.TalkId);
                    Assert.NotNull(meetupSession.StartTime);
                    Assert.NotNull(meetupSession.EndTime);
                });
            }
        }

        [Fact]
        public async Task ShouldHaveValidSpeakers()
        {
            // Arrange
            if (session == null)
            {
                return;
            }

            // Act
            var speakers = await session.QueryAsync<Speaker>().ToListAsync().ConfigureAwait(false);

            // Assert
            Assert.NotEmpty(speakers);

            foreach (var speaker in speakers)
            {
                Assert.NotNull(speaker.Id);
                Assert.NotNull(speaker.Name);
                Assert.NotNull(speaker.Description);
            }
        }

        [Fact]
        public async Task ShouldHaveValidTalks()
        {
            // Arrange
            if (session == null)
            {
                return;
            }

            // Act
            var talks = await session.QueryAsync<Talk>().ToListAsync().ConfigureAwait(false);

            // Assert
            Assert.NotEmpty(talks);

            foreach (var talk in talks)
            {
                Assert.NotNull(talk.Id);
                Assert.NotNull(talk.Name);
                Assert.NotNull(talk.Description);
                Assert.NotEmpty(talk.SpeakerIds);
            }
        }

        [Fact]
        public async Task ShouldHaveValidVenues()
        {
            // Arrange
            if (session == null)
            {
                return;
            }

            // Act
            var venues = await session.QueryAsync<Venue>().ToListAsync().ConfigureAwait(false);

            // Assert
            Assert.NotEmpty(venues);

            foreach (var venue in venues)
            {
                Assert.NotNull(venue.Id);
                Assert.NotNull(venue.Name);
                Assert.NotNull(venue.Address);
                Assert.NotNull(venue.MapUrl);
            }
        }

        [Fact]
        public async Task ShouldHaveValidFriends()
        {
            // Arrange
            if (session == null)
            {
                return;
            }

            // Act
            var friends = await session.QueryAsync<Friend>().ToListAsync().ConfigureAwait(false);

            // Assert
            Assert.NotEmpty(friends);

            foreach (var friend in friends)
            {
                Assert.NotNull(friend.Id);
                Assert.NotNull(friend.Name);
                Assert.NotNull(friend.Url);
                Assert.NotNull(friend.Description);
            }
        }
    }
}
