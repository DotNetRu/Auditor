using System;
using System.Threading.Tasks;
using DotNetRu.Auditor.Storage.Collections;
using DotNetRu.Auditor.Storage.Collections.Bindings;
using DotNetRu.Auditor.Storage.FileSystem;
using Moq;
using Xunit;

namespace DotNetRu.Auditor.UnitTests.Storage.Collections.Bindings
{
    public sealed class CompositeMatcherTest
    {
        [Fact]
        public void ShouldMatchSingle()
        {
            // Arrange
            const string expectedCollectionName = nameof(ShouldMatchSingle);

            var matcher1 = ConstMatcher.None();
            var matcher2 = ConstMatcher.Some(expectedCollectionName);
            var matcher3 = ConstMatcher.None();

            var compositeMatcher = new CompositeMatcher(new[] { matcher1, matcher2, matcher3 });

            // Act
            var matchedCollection = compositeMatcher.Match();

            // Assert
            Assert.Null(compositeMatcher.ErrorMessage);

            var collection = AssertEx.NotNull(matchedCollection);
            Assert.Equal(expectedCollectionName, collection.Name);
        }

        [Fact]
        public void ShouldNotMatchZero()
        {
            // Arrange
            var matcher1 = ConstMatcher.Some(nameof(ShouldNotMatchZero) + "1");
            var matcher2 = ConstMatcher.Some(nameof(ShouldNotMatchZero) + "2");
            var matcher3 = ConstMatcher.Some(nameof(ShouldNotMatchZero) + "3");

            var compositeMatcher = new CompositeMatcher(new[] { matcher1, matcher2, matcher3 });

            // Act
            var matchedCollection = compositeMatcher.Match();

            // Assert
            Assert.Null(matchedCollection);
            Assert.NotNull(compositeMatcher.ErrorMessage);
        }

        [Fact]
        public void ShouldNotMatchMany()
        {
            // Arrange
            var compositeMatcher = new CompositeMatcher(Array.Empty<Matcher>());

            // Act
            var matchedCollection = compositeMatcher.Match();

            // Assert
            Assert.Null(matchedCollection);
            Assert.NotNull(compositeMatcher.ErrorMessage);
        }

        private sealed class ConstMatcher : Matcher
        {
            private readonly string? name;

            private ConstMatcher(string? name)
            {
                this.name = name;
            }

            public static ConstMatcher Some(string name) => new(name);
            public static ConstMatcher None() => new(null);

            public override Task AcceptAsync(IFile file) => throw new NotSupportedException();
            public override Task AcceptAsync(IDirectory directory) => throw new NotSupportedException();

            public override Collection? Match()
            {
                if (name == null)
                {
                    return null;
                }

                var directory = new Mock<IDirectory>(MockBehavior.Strict);
                directory.Setup(d => d.Name).Returns(name);

                var collection = new Mock<Collection>(MockBehavior.Strict, directory.Object);
                return collection.Object;
            }
        }
    }
}
