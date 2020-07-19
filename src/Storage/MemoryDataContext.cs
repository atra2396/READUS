using DomainObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Storage
{
    public class MemoryDataContext : IDataContext
    {
        public IDataRepository<Document> Documents { get; }
        public IDataRepository<Organization> Organizations { get; }
        public IDataRepository<Repository> Repositories { get; }

        public MemoryDataContext()
        {
            this.Documents = new MemoryRepository<Document>();
            this.Organizations = new MemoryRepository<Organization>();
            this.Repositories = new MemoryRepository<Repository>();
        }

        public MemoryDataContext(IDataRepository<Document> docs, IDataRepository<Organization> orgs, IDataRepository<Repository> repos)
        {
            this.Documents = docs;
            this.Organizations = orgs;
            this.Repositories = repos;
        }
    }
}
