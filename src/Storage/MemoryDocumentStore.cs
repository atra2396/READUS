using DomainObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Storage
{
    public class MemoryDocumentStore : IDocumentReader, IDocumentWriter
    {
        Dictionary<Guid, Document> docByDocId;

        public MemoryDocumentStore()
        {
            this.docByDocId = new Dictionary<Guid, Document>();
        }

        public Document GetDocument(Guid orgId, Guid docId)
        {
            if (this.docByDocId.ContainsKey(docId))
                return this.docByDocId[docId].OrganizationId == orgId ? this.docByDocId[docId] : null;

            return null;
        }

        public Document GetDocument(Guid orgId, string path)
        {
            foreach(var doc in docByDocId.Values)
            {
                if (doc.OrganizationId == orgId && doc.Path == path)
                    return doc;
            }

            return null;
        }

        public Guid CreateDocument(Document document)
        {
            var newId = Guid.NewGuid();
            document = new Document(newId, document.OrganizationId, document.Path, document.Body);
            this.docByDocId.Add(newId, document);

            return newId;
        }
    }
}
