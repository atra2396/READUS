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

        // auth, uri, etc... whatever the SCM needs, stored as an encrypted json
        public string CustomRepositoryInformation { get; set; }
        public SupportedSystems SCM { get; set; }

        public IEnumerable<Document> Documents { get; }
    }
}
