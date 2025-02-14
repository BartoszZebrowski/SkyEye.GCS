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
        private static readonly Lazy<TCPMessageClient> _instance = new(() => new TCPMessageClient());
        public static TCPMessageClient Instance => _instance.Value;

        public event Action<string>? MessageReceived;

        private TcpClient? _client;
        private NetworkStream? _stream;
        private IEnumerable<IMessageResponse> _messageResponses;

        private TCPMessageClient() { }

        public void SetMessageResponses(IEnumerable<IMessageResponse> messageResponses) 
            => _messageResponses = messageResponses;

        public async Task ConnectAsync(string ip, int port)
        {
            _client = new TcpClient();
            await _client.ConnectAsync(ip, port);
            _stream = _client.GetStream();

            StartListening();
        }

        private async void StartListening()
        {
            byte[] buffer = new byte[1024];

            while (_client?.Connected == true)
            {
                int bytesRead = await _stream!.ReadAsync(buffer);
                if (bytesRead > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    MessageReceived?.Invoke(message);
                }
            }
        }

        public async Task SendMessageAsync(IMessageRequest message)
        {
            if (_stream == null) 
                throw new InvalidOperationException("Not connected");

            await _stream.WriteAsync(message.Serialize());
        }
    }
}
