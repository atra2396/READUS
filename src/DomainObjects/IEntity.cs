using System;
using System.Collections.Generic;
using System.Text;

namespace DomainObjects
{
    public interface IEntity
    {
        Guid Id { get; set; }
        DateTime Created { get; set; }
        DateTime Updated { get; set; }

    }
}
