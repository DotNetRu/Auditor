using System.IO;
using System.Threading.Tasks;
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
    }
}
