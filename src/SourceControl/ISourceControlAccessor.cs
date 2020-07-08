using DomainObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace SourceControl
{
    public interface ISourceControlAccessor
    {
        IEnumerable<Document> GetReadmes(Repository repository);
    }
}
