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
            // "C:\" + "Abc" => "C:\Abc"
            yield return new object[] {filesystemRoot, "Abc", Path.Combine(filesystemRoot, "Abc")};
            // "C:\1" + ".\Abc" => "C:\1\Abc"
            yield return new object[] {Path.Combine(filesystemRoot, "1"), Path.Combine(".","Abc"), Path.Combine(filesystemRoot, "1", "Abc")};
        }
    }
}
