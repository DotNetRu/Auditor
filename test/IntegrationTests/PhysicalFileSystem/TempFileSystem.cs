using System;
using System.IO;
using System.Linq;
using DotNetRu.Auditor.Storage.FileSystem;
using DotNetRu.Auditor.Storage.FileSystem.Physical;

namespace DotNetRu.Auditor.IntegrationTests.PhysicalFileSystem
{
    public sealed class TempFileSystem : IDisposable
    {
        private static readonly string TempDirectoryPrefix = (typeof(IsRoot).Namespace ?? throw new InvalidOperationException());

        private TempFileSystem(IDirectory root)
        {
            Root = root;
        }

        public IDirectory Root { get; }

        public static TempFileSystem Create()
        {
            DeleteTempDirectories();
            var tempPath = CreateTempDirectory();
            var tempDirectory = new PhysicalDirectory(tempPath);
            return new TempFileSystem(tempDirectory);
        }

        public void Dispose()
        {
            Directory.Delete(Root.FullName, true);
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
