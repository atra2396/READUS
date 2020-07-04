using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DomainObjects
{
    public class Document
    {
        public Document(Guid docId, Guid orgId, string path, string body)
        {
            this.Id = docId;
            this.OrganizationId = orgId;
            this.Path = path;
            this.Body = body;
        }

        public Guid Id { get; }
        public Guid OrganizationId { get; }
        public string Path { get; }
        public string Body { get; }
    }
}
