using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEye.Connector.MessagesService.Messages.Abstractions
{
    public interface IMessageResponse<T>
    {
        event Action<string> MessageRecived;
        MessageType MessageType { get; }
        T Value { get; }
        T Deserialize(byte[] response);
    }
}
