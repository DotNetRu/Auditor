using System.Collections.Generic;
using System.IO;

namespace DotNetRu.Auditor.Tests.Storage.FileSystem
{
    public class CrossPlatformDataGenerator
    {
        private static readonly string filesystemRoot;

        static CrossPlatformDataGenerator()
        {
            filesystemRoot = Path.GetPathRoot(Directory.GetCurrentDirectory()) ?? string.Empty;
        }

        public static IEnumerable<object[]> GetDataForFullPathTest()
        {
            yield return new object[] {filesystemRoot, "Abc", Path.Combine(filesystemRoot, "Abc")};
            yield return new object[] {Path.Combine(filesystemRoot, "1"), @".\Abc", Path.Combine(filesystemRoot, "1", "Abc")};
        }
    }
}
