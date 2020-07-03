using System;
using System.Threading.Tasks;

namespace Queueing
{
    public interface IQueue<TMessageType>
    {
        void PushMessage(TMessageType message);
        TMessageType PopMessage();
    }
}
