using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using SkyEye.Connector.MessagesService;

namespace SkyEye.Connector.Datalink
{
    public class WebsocketClient
    {

        private ClientWebSocket _clientWebSocket;
        private Uri _serverUri;
        private Datalink _datalink;

        public WebsocketClient(string adressIp, int port, Datalink datalink)
        {
            _datalink = datalink;
            _clientWebSocket = new ClientWebSocket();
            _serverUri = new Uri($"ws://{adressIp}:{port}");
            _clientWebSocket.ConnectAsync(_serverUri, CancellationToken.None);
        }

        public async void UpdateRemoteValues()
        {
            foreach (var remoteValue in _datalink.RemoteValues.Where(rm => rm.RemoteValueMode != RemoteValueMode.WriteOnly))
                await GetValue(remoteValue);

            foreach (var remoteValue in _datalink.RemoteValues.Where(rm => rm.RemoteValueMode != RemoteValueMode.ReadOnly && rm.ToUpdate))
                await SetValue(remoteValue);
        }

        internal async Task GetValue(IRemoteValue remoteValue)
        {
            string message = $"{(int)remoteValue.RemoteValueType};0;0";

            var response = (await SendAndReciveAsync(message));

            if (remoteValue is RemoteValue<int> intRemoteValue)
            {
                intRemoteValue.UpdateValue(int.Parse(response));
            }
            else if (remoteValue is RemoteValue<float> floatRemoteValue)
            {
                floatRemoteValue.UpdateValue(float.Parse(response));
            }
            else if (remoteValue is RemoteValue<string> stringRemoteValue)
            {
                stringRemoteValue.UpdateValue(response);
            }
            else if (remoteValue is RemoteValue<object> genericRemoteValue)
            {
                Type type = genericRemoteValue.Value.GetType();

                if (type.IsEnum)
                {
                    genericRemoteValue.UpdateValue(Enum.Parse(type, response));
                }
                else
                {
                    throw new InvalidOperationException($"Nieobsługiwany typ: {type}");
                }
            }
        }

        internal async Task SetValue(IRemoteValue remoteValue)
        {
            string message = "";

            if (remoteValue is RemoteValue<int> intRemoteValue)
            {
                message = $"{(int)remoteValue.RemoteValueType};1;{intRemoteValue.GetValueToUpdate().ToString()}";
            }
            else if (remoteValue is RemoteValue<float> floatRemoteValue)
            {
                message = $"{(int)remoteValue.RemoteValueType};1;{floatRemoteValue.GetValueToUpdate().ToString()}";
            }
            else if (remoteValue is RemoteValue<string> stringRemoteValue)
            {
                message = $"{(int)remoteValue.RemoteValueType};1;{stringRemoteValue.GetValueToUpdate().ToString()}";
            }
            else if (remoteValue is RemoteValue<object> enumRemoteValue)
            {
                message = $"{(int)remoteValue.RemoteValueType};1;{(int)enumRemoteValue.GetValueToUpdate()}";
            }

            await SendAndReciveAsync(message);

            remoteValue.ToUpdate = false;
        }

        private async Task<string> SendAndReciveAsync(string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            5await _clientWebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);

            byte[] receiveBuffer = new byte[1024];
            WebSocketReceiveResult result = await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
            string receivedMessage = Encoding.UTF8.GetString(receiveBuffer, 0, result.Count);

            return receivedMessage;
        }
    }
}
