using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using DotNetRu.Auditor.Storage;
using DotNetRu.Auditor.Storage.FileSystem;
using Xunit;

namespace DotNetRu.Auditor.IntegrationTests.Audit
{
    [CollectionDefinition(Name)]
    public sealed class AuditFixture : ICollectionFixture<AuditFixture>
    {
        public const string Name = nameof(AuditFixture);

        public AuditFixture()
        {
            var currentPath = Path.GetDirectoryName(typeof(AuditFixture).Assembly.Location);
            var auditPath = Path.Combine("Audit", "db");
            if (currentPath != null && TryFindDirectoryAbove(currentPath, auditPath, out var auditFullName))
            {
                var auditDirectory = PhysicalFileSystem.ForDirectory(auditFullName);
                Store = AuditStore.OpenAsync(auditDirectory).GetAwaiter().GetResult();
            }
            else
            {
                // Audit directory cannot be found in the local file system. But this is normal for most cases.
                Store = default;
            }
        }

        public IStore? Store { get; }

        private static bool TryFindDirectoryAbove(string startDirectoryFullName, string targetDirectoryName, [NotNullWhen(true)] out string? targetDirectoryFullName)
        {
            var parent = startDirectoryFullName;
            while (parent != null)
            {
                targetDirectoryFullName = Path.Combine(parent, targetDirectoryName);
                if (Directory.Exists(targetDirectoryFullName))
                {
                    return true;
                }

                parent = Path.GetDirectoryName(parent);
            }

            targetDirectoryFullName = default;
            return false;
        }
    }
}
