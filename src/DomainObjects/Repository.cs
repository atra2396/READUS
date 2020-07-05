using System;
using System.Collections.Generic;

namespace DomainObjects
{
    public class Repository : IDomainObject
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public Guid OrganizationId { get; set; }
        public string Name { get; set; }
        public IEnumerable<Document> Documents { get; }
    }
}
