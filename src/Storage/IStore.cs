namespace DotNetRu.Auditor.Storage
{
    public interface IStore
    {
        IUnitOfWork OpenSession();

        IReadOnlySession OpenReadOnlySession();
    }
}
