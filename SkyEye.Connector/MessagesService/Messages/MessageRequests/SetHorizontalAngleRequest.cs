using SkyEye.Connector.MessagesService.Messages.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SkyEye.Connector.MessagesService.Messages.MessageRequests
{
    public class SetHorizontalAngleRequest : IMessageRequest
    {
        public MessageType CommandType { get => MessageType.HorizontalAngle; }

        private string _message;
        public string Message
        {
            get => _message;
        }

        public MessageType MessageType => MessageType.HorizontalAngle;

        public SetHorizontalAngleRequest(string message)
        {
            _message = message;
        }
    }
}
