using SkyEye.Connector.MessagesService.Messages.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEye.Connector.MessagesService.Messages.MessageResposnes
{
    public class HorizontalPositionResponse : IMessageResponse
    {
        public event Action<string> MessageRecived;

        public int HorizontalAngle = 0;
        public MessageType MessageType => MessageType.HorizontalAngle;

        public void Deserialize(byte[] response)
        {
            HorizontalAngle = int.Parse(response);
        }
    }
}
