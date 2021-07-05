using System;
using System.IO;
using System.Linq;

namespace DotNetRu.Auditor.IntegrationTests.FileSystem
{
    public sealed class TempFileSystem : IDisposable
    {
        private static readonly string TempDirectoryPrefix = (typeof(IsRoot).Namespace ?? throw new InvalidOperationException());

        private TempFileSystem(string root)
        {
            Root = root;
        }

        public string Root { get; }

        public static TempFileSystem Create()
        {
            DeleteTempDirectories();
            var tempPath = CreateTempDirectory();
            return new TempFileSystem(tempPath);
        }

        public void Dispose()
        {
            Directory.Delete(Root, true);
        }

        private static string GetSystemTempPath() => Path.GetTempPath();

        private static string CreateTempDirectory()
        {
            var systemTempPath = GetSystemTempPath();
            var appTempDirectory = Enumerable
                .Range(0, 100)
                .Select(index => $"{TempDirectoryPrefix}-{index}")
                .Select(name => Path.Combine(systemTempPath, name))
                .FirstOrDefault(path => !Directory.Exists(path));

            if (appTempDirectory == null)
            {
                throw new InvalidOperationException($"Can't find free name at «{systemTempPath}»");
            }

            Directory.CreateDirectory(appTempDirectory);

            return appTempDirectory;
        }

        private static void DeleteTempDirectories()
        {
            static bool IsOldDirectory(string path)
            {
                var activeTime = new DirectoryInfo(path).LastAccessTimeUtc;
                var oneHourAgo = DateTime.UtcNow.AddHours(-1);
                return activeTime < oneHourAgo;
            }

            var systemTempPath = GetSystemTempPath();
            var appTempPaths = Directory
                .EnumerateDirectories(systemTempPath, $"{TempDirectoryPrefix}*")
                .Where(IsOldDirectory);

            foreach (var appTempPath in appTempPaths)
            {
                Directory.Delete(appTempPath, true);
            }
        }
    }
}
