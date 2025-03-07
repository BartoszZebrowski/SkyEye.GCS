using SkyEye.Connector.CommandService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEye.Connector.MessagesService
{
    public class RemoteValue<T>
    {
        public RemoteValueType RemoteValueType;
        private RemoteValueResponseType RemoteValueResponseType;

        private TCPMessageClient _tcpMessageClient;
        private T _value = default;

        public RemoteValue(TCPMessageClient tcpMessageClient,
            RemoteValueType remoteValueType,
            RemoteValueResponseType remoteValueResponseType = RemoteValueResponseType.WithResponse)
        {
            _tcpMessageClient = tcpMessageClient;
            RemoteValueResponseType = remoteValueResponseType;
            RemoteValueType = remoteValueType;
        }

        public async Task<T> Get()
        {
            _value = await _tcpMessageClient.GetValue<T>(RemoteValueType);
            return _value;
        }

        public async Task Set(T value)
        {
            _value = await _tcpMessageClient.SetValue<T>(RemoteValueType, value);
        }
    }
}
