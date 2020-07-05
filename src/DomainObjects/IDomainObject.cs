using System;
using System.Collections.Generic;
using System.Text;

namespace DomainObjects
{
    public interface IDomainObject
    {
        Guid Id { get; set; }
        DateTime Created { get; set; }
        DateTime Updated { get; set; }

    }
}
