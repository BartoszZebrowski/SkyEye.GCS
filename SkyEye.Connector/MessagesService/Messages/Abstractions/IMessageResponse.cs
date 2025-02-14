using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEye.Connector.MessagesService.Messages.Abstractions
{
    public interface IMessageResponse
    {
        event Action<string> MessageRecived;

        MessageType MessageType { get; }
        void Deserialize(byte[] response);
    }
}
