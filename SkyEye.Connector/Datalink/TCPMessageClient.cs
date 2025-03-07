using Gst.Rtsp;
using SkyEye.Connector.MessagesService;
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

        public TCPMessageClient(string ip, int port) 
        {
            ConnectAsync(ip, port);
        }

        public void ConnectAsync(string ip, int port)
        {
            //_client = new TcpClient();
            //_client.Connect(ip, port);
            //_stream = _client.GetStream();
        }
        internal async Task<T> GetValue<T>(RemoteValueType remoteValueType)
        {
            string message = $"{(int)remoteValueType};0;0";

            var response = (await SendAndReciveAsync(message));

            if (typeof(T) == typeof(int))
            {
                return (T)(object)int.Parse(response);
            }
            else if (typeof(T) == typeof(string))
            {
                return (T)(object)response;
            }
            else if (typeof(T).IsEnum)
            {
                return (T)Enum.Parse(typeof(T), response);
            }
            else
            {
                throw new InvalidOperationException($"Nieobsługiwany typ: {typeof(T)}");
            }
        }

        internal async Task<T> SetValue<T>(RemoteValueType remoteValueType, T value)
        {
            string message = $"{(int)remoteValueType};1;{value}";

            var response = (await SendAndReciveAsync(message));

            if (typeof(T) == typeof(int))
            {
                return (T)(object)int.Parse(response);
            }
            else if (typeof(T) == typeof(string))
            {
                return (T)(object)response;
            }
            else if (typeof(T).IsEnum)
            {
                return (T)Enum.Parse(typeof(T), response);
            }
            else
            {
                throw new InvalidOperationException($"Nieobsługiwany typ: {typeof(T)}");
            }
        }

        private async Task<string> SendAndReciveAsync(string message)
        {
            await _stream.WriteAsync(Encoding.UTF8.GetBytes(message));

            byte[] buffer = new byte[1024];

            while (_client?.Connected == true)
            {
                int bytesRead = await _stream!.ReadAsync(buffer);
                if (bytesRead > 0)
                {
                    return Encoding.UTF8.GetString(buffer, 0, bytesRead);
                }
            }

            throw new Exception("Cos sie zjebalo");
        }


    }
}
