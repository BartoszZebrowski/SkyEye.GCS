using SkyEye.Connector.MessagesService.Messages.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEye.Connector.MessagesService.Messages.MessageRequests
{
    internal class SetVerticalAngleRequest
    {
        public MessageType CommandType { get => MessageType.HorizontalAngle; }

        private string _message;

        public SetVerticalAngleRequest(string message)
        {
            _message = message;
        }

        public string Message
        {
            get => _message;
        }
    }
}
