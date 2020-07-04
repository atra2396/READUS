using DomainObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Storage
{
    public interface IDocumentWriter
    {
        Guid CreateDocument(Document document);
    }
}
