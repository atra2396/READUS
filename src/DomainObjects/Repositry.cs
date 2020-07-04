using System;
using System.Collections.Generic;

namespace DomainObjects
{
    public class Repository
    {
        public IEnumerable<Document> Documents { get; }
    }
}
