using DomainObjects;
using System;

namespace Storage
{
    public interface IDocumentReader
    {
        Document GetDocument(Guid orgId, Guid docId);
        Document GetDocument(Guid orgId, string path);
    }
}
