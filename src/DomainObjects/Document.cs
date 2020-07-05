using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DomainObjects
{
    public class Document : IEntity
    {

        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public Guid RepositoryId { get; set; }
        public string Path { get; set; }
        public string Body { get; set; }
    }
}
