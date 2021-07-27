namespace DotNetRu.Auditor.Storage.Collections
{
    public sealed class SessionOptions
    {
        // TDO: Add change tracking
        public bool TrackChanges { get; set; }

        // TDO: Cache deserialized records
        public bool CacheRecords { get; set; } = true;
    }
}
