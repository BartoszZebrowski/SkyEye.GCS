using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEye.Connector.MessagesService.Messages.Abstractions
{
    public interface IMessageRequest
    {
        MessageType MessageType { get; }
        string Message { get; }
        byte[] Serialize()
            => Encoding.UTF8.GetBytes((int)MessageType + ":" + Message);
    }
}
