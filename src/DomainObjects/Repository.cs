using System;
using System.Collections.Generic;

namespace DomainObjects
{
    public class Repository : IEntity
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public Guid OrganizationId { get; set; }
        public string Name { get; set; }

        // uri generally, or a path for local testing?
        public string RootLocation { get; set; }

        public IEnumerable<Document> Documents { get; }
    }
}
