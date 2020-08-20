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
        public IDataRepository<User> Users { get; }

        public MemoryDataContext()
        {
            this.Documents = new MemoryRepository<Document>();
            this.Organizations = new MemoryRepository<Organization>();
            this.Repositories = new MemoryRepository<Repository>();
            this.Users = new MemoryRepository<User>();
        }

        public MemoryDataContext(IDataRepository<Document> docs,
            IDataRepository<Organization> orgs,
            IDataRepository<Repository> repos,
            IDataRepository<User> users)
        {
            this.Documents = docs;
            this.Organizations = orgs;
            this.Repositories = repos;
            this.Users = users;
        }
    }
}
