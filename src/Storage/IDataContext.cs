using DomainObjects;

namespace Storage
{
    public interface IDataContext
    {
        IDataRepository<Document> Documents { get; }
        IDataRepository<Organization> Organizations { get; }
        IDataRepository<Repository> Repositories { get; }
    }
}