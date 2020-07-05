using DomainObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Storage
{
    public class DataContext
    {
        public IDataRepository<Document> Documents { get; }
        public IDataRepository<Organization> Organizations { get; }
        public IDataRepository<Repository> Repositories { get; }

        public DataContext(IDataRepository<Document> docRepo, IDataRepository<Organization> orgRepo, IDataRepository<Repository> repoRepo)
        {
            this.Documents = docRepo;
            this.Organizations = orgRepo;
            this.Repositories = repoRepo;
        }
    }
}
