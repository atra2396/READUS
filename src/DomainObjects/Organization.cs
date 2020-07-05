using System;
using System.Collections.Generic;
using System.Text;

namespace DomainObjects
{
    public class Organization : IDomainObject
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        // The Id given by the source, for example "SubscriptionId" from Azure DevOps
        public string SourceId { get; set; }
        public string Name { get; set; }
        public IEnumerable<Repository> Repositories { get; set; }

    }
}
