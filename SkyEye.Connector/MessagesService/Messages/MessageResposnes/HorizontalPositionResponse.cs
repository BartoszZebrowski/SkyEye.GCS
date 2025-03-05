using SkyEye.Connector.MessagesService.Messages.Abstractions;
using SkyEye.Connector.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEye.Connector.MessagesService.Messages.MessageResposnes
{
    public class HorizontalPositionResponse : IMessageResponse<Angle>
    {
        public MessageType MessageType => throw new NotImplementedException();

        public Angle Value => throw new NotImplementedException();

        public event Action<string> MessageRecived;

        public Angle Deserialize(byte[] response)
        {
            throw new NotImplementedException();
        }
    }
}
