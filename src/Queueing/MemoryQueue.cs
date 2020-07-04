using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queueing
{
    public class MemoryQueue<TMessageType> : IQueue<TMessageType>
    {
        List<TMessageType> queue;

        public MemoryQueue()
        {
            this.queue = new List<TMessageType>();
        }

        public bool HasMessages()
        {
            return this.queue.Any();
        }

        public TMessageType PopMessage()
        {
            var message = this.queue[0];
            this.queue.RemoveAt(0);
            return message;
        }

        public void PushMessage(TMessageType message)
        {
            this.queue.Add(message);

        }
    }
}
