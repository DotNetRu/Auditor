using System.IO;

namespace DotNetRu.Auditor.Storage.Collections.Xml
{
    internal static class XmlPath
    {
        public const string IndexFileName = "index.xml";

        public const string Extension = "xml";

        public static string ChangeExtension(string path) =>
            Path.ChangeExtension(path, Extension);

        public static bool HasExtension(string fileName) =>
            Path.GetExtension(fileName) == "." + Extension;
    }
}
