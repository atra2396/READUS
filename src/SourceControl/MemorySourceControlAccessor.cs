using DomainObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace SourceControl
{
    public class MemorySourceControlAccessor : ISourceControlAccessor
    {
        public MemorySourceControlAccessor()
        {

        }

        public IEnumerable<Document> GetReadmes(Repository repository)
        {
            
        }
    }
}
