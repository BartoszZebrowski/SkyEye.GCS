using SkyEye.Connector.CommandService;
using SkyEye.Connector.MessagesService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEye.Connector.Datalink
{
    public class Datalink
    {
        private TCPMessageClient _tcpMessageClient;
        private List<object> _remoteValues = new();

        public Datalink(TCPMessageClient tcpMessageClient)
        {
            _tcpMessageClient = tcpMessageClient;
            CreateRemoteValues();
        }

        public RemoteValue<T> GetRemoteValue<T>(RemoteValueType remoteValueType)
        {
            return (RemoteValue<T>)_remoteValues.Where(remoteValue => ((RemoteValue<T>)remoteValue).RemoteValueType == remoteValueType).First();
        }

        private void CreateRemoteValues()
        {
            _remoteValues.Add(new RemoteValue<int>(_tcpMessageClient, RemoteValueType.HorisonalAxis));
            _remoteValues.Add(new RemoteValue<int>(_tcpMessageClient, RemoteValueType.VerticalAxis));
            _remoteValues.Add(new RemoteValue<int>(_tcpMessageClient, RemoteValueType.WorkingMode));
        }
    }
}
