using DomainObjects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SourceControl
{
    public interface ISourceControlAccessor
    {
        Task<IEnumerable<Document>> GetReadmes(Repository repository);
    }
}
