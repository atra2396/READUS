using System;
using System.Collections.Generic;
using System.Text;

namespace DomainObjects
{
    public class Organization
    {
        public Organization(Guid id, string name, IEnumerable<Repository> repositories)
        {
            this.Id = id;
            this.Name = name;
            this.Repositories = repositories;
        }

        public Guid Id { get; }
        public string Name { get; }
        public IEnumerable<Repository> Repositories { get; }
    }
}
