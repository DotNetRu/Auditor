namespace DotNetRu.Auditor.Storage.FileSystem.Memory
{
    internal sealed class MemoryFile
    {
        public ReusableStream Content { get; } = new();
    }
}
