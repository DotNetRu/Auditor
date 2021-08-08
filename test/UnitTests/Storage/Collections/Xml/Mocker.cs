using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotNetRu.Auditor.Data.Description;
using DotNetRu.Auditor.Data.Model;
using DotNetRu.Auditor.Storage.Collections.Xml;
using DotNetRu.Auditor.Storage.FileSystem;
using Moq;

namespace DotNetRu.Auditor.UnitTests.Storage.Collections.Xml
{
    internal static class Mocker
    {
        public static Mock<IDirectory> MockDirectory(string fullName, bool exists = true)
        {
            var name = Path.GetFileName(fullName);
            var directory = new Mock<IDirectory>(MockBehavior.Strict);
            directory.Setup(d => d.Name).Returns(name);
            directory.Setup(d => d.FullName).Returns(fullName);
            directory.Setup(d => d.ExistsAsync()).Returns(() => ValueTask.FromResult(exists));
            return directory;
        }

        public static Mock<IFile> MockFile(string fullName, bool exists = true)
        {
            var name = Path.GetFileName(fullName);
            var file = new Mock<IFile>(MockBehavior.Strict);
            file.Setup(f => f.Name).Returns(name);
            file.Setup(f => f.FullName).Returns(fullName);
            file.Setup(f => f.ExistsAsync()).Returns(() => ValueTask.FromResult(exists));
            return file;
        }

        public static ModelDefinition GetCommunityModel(this ModelRegistry registry)
        {
            var communityModel = registry.Models.First(model => model.Name == nameof(Community));
            return communityModel;
        }

        public static string GetCommunityCollectionName(this ModelRegistry registry)
        {
            var communityModel = registry.GetCommunityModel();
            return XmlCollectionFactory.ToCollectionName(communityModel.GroupName);
        }

        public static Community Community(string id, string? name = default)
        {
            return new()
            {
                Id = id,
                Name = name ?? id
            };
        }

        public static string CommunityState(string id, string? name = default)
        {
            return $@"
<Community>
  <Id>{id}</Id>
  <Name>{name ?? id}</Name>
</Community>";
        }
    }
}
