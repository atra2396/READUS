using System;
using System.Collections.Generic;
using System.Text;

namespace Queueing.Messages
{
    public class RepositoryUpdatedMessage
    {
        public RepositoryUpdatedMessage(Guid id)
        {
            this.RepositoryId = id;
        }

        public Guid RepositoryId { get; }
    }
}
