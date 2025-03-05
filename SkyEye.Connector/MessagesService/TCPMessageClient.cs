using Gst.Rtsp;
using SkyEye.Connector.MessagesService.Messages.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SkyEye.Connector.CommandService
{
    public class TCPMessageClient
    {
        public event Action<string>? MessageReceived;

        private TcpClient? _client;
        private NetworkStream? _stream;
        private IEnumerable<IMessageResponse<object>> _messageResponses;

        public TCPMessageClient() { }

        public void SetMessageResponses(IEnumerable<IMessageResponse<object>> messageResponses) 
            => _messageResponses = messageResponses;

        public async Task ConnectAsync(string ip, int port)
        {
            _client = new TcpClient();
            _client.Connect(ip, port);
            _stream = _client.GetStream();
        }

        private async Task StartListening()
        {
            byte[] buffer = new byte[1024];

            while (_client?.Connected == true)
            {
                int bytesRead = await _stream!.ReadAsync(buffer);
                if (bytesRead > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    MessageReceived?.Invoke(message);
                    break;
                }
            }
        }

        public async Task SendMessageAsync(IMessageRequest message)
        {
            if (_stream == null) 
                throw new InvalidOperationException("Not connected");

            await _stream.WriteAsync(message.Serialize());

            await StartListening();
        }
    }
}
