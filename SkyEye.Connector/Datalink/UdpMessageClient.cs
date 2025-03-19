using SkyEye.Connector.Datalink;
using SkyEye.Connector.MessagesService;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Task = System.Threading.Tasks.Task;


namespace SkyEye.Connector.CommandService
{
    public class UdpMessageClient
    {
        private UdpClient? _client;
        private Datalink.Datalink _datalink;
        private IPEndPoint _remoteEP = new IPEndPoint(IPAddress.Any, 0);
        private ConcurrentQueue<string> _remoteVariableToSetQueue = new();

        public UdpMessageClient(string ip, int port, Datalink.Datalink datalink) 
        {
            _datalink = datalink;
            Connect(ip, port);
        }

        public void Connect(string ip, int port)
        {
            _client = new UdpClient();
            _client.Connect(ip, port);

            Task.Run(() => SendLoop());
            Task.Run(() => ReciveLoop());
        }

        public void SynchroniseRemoteValues()
        {
            foreach (var remoteValue in _datalink.RemoteValues)
            {
                _remoteVariableToSetQueue.Enqueue(SerializeGetValueMessage(remoteValue));

                if (remoteValue.ToUpdate)
                {
                    _remoteVariableToSetQueue.Enqueue(SerializeSetValueMessage(remoteValue));
                    remoteValue.ToUpdate = false;
                }
            }
        }

        private void SendLoop()
        {
            while (true)
            {
                if (!_remoteVariableToSetQueue.TryDequeue(out string firstElement))
                    continue;

                _client.SendAsync(Encoding.UTF8.GetBytes(firstElement));
            }
        }

        private void ReciveLoop()
        {
            while (true)
            {
                var recivedMessage = _client.Receive(ref _remoteEP);
                var message = Encoding.UTF8.GetString(recivedMessage);
                DeserializeRemoteValue(message);
            }
        }

        private string SerializeSetValueMessage(IRemoteValue remoteValue)
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

            return message;
        }

        private string SerializeGetValueMessage(IRemoteValue remoteValue)
        {
            return $"{(int)remoteValue.RemoteValueType};0;0";
        }

        private void DeserializeRemoteValue(string message)
        {
            var splitedMessage = message.Split(";") 
                ?? throw new Exception("Wrong data format");

            var remoteValueType = int.Parse(splitedMessage[0]);
            var stringValue = splitedMessage[1];

            var remoteValue = _datalink.RemoteValues.Where(rm => (int)rm.RemoteValueType == remoteValueType).First() 
                ?? throw new Exception("This remote value don't exist");

            UpdateValue(remoteValue, stringValue);
        }

        private void UpdateValue(IRemoteValue remoteValue, string value) 
        {
            if (remoteValue is RemoteValue<int> intRemoteValue)
            {
                intRemoteValue.UpdateValue(int.Parse(value));
            }
            else if (remoteValue is RemoteValue<float> floatRemoteValue)
            {
                floatRemoteValue.UpdateValue(float.Parse(value));
            }
            else if (remoteValue is RemoteValue<string> stringRemoteValue)
            {
                stringRemoteValue.UpdateValue(value);
            }
            else if (remoteValue is RemoteValue<object> genericRemoteValue)
            {
                Type type = genericRemoteValue.Value.GetType();

                if (type.IsEnum)
                {
                    genericRemoteValue.UpdateValue(Enum.Parse(type, value));
                }
                else
                {
                    throw new InvalidOperationException($"Nieobsługiwany typ: {type}");
                }
            }
        }
    }
}
