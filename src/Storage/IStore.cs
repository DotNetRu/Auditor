namespace DotNetRu.Auditor.Storage
{
    public interface IStore
    {
        ISession OpenSession();

        IReadOnlySession OpenReadOnlySession();
    }
}
