using System.Threading.Tasks;

namespace DotNetRu.Auditor.Storage
{
    public interface IUnitOfWork : ISession
    {
        Task SaveChangesAsync();
    }
}
