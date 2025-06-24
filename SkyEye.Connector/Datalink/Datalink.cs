using SkyEye.Connector.CommandService;
using SkyEye.Connector.MessagesService;
using SkyEye.Connector.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEye.Connector.Datalink
{
    public class Datalink
    {
        public List<IRemoteValue> RemoteValues { get; private set; } = new();

        public Datalink()
        {
            CreateRemoteValues();
        }

        public RemoteValue<T> GetRemoteValue<T>(RemoteValueType remoteValueType)
        {
            return (RemoteValue<T>)RemoteValues.Where(remoteValue => remoteValue.RemoteValueType == remoteValueType).First();
        }

        private void CreateRemoteValues()
        {
            RemoteValues.Add(new RemoteValue<int>(RemoteValueType.Ping, RemoteValueMode.ReadAndWrite));
            RemoteValues.Add(new RemoteValue<int>(RemoteValueType.WorkingMode, RemoteValueMode.ReadAndWrite));
            RemoteValues.Add(new RemoteValue<float>(RemoteValueType.TargetHorizontalAngle, RemoteValueMode.ReadAndWrite));
            RemoteValues.Add(new RemoteValue<float>(RemoteValueType.TargetVerticalAngle, RemoteValueMode.ReadAndWrite));
            RemoteValues.Add(new RemoteValue<float>(RemoteValueType.ActualHorizontaAngle, RemoteValueMode.ReadOnly));
            RemoteValues.Add(new RemoteValue<float>(RemoteValueType.ActualVerticalAngle, RemoteValueMode.ReadOnly));
            RemoteValues.Add(new RemoteValue<float>(RemoteValueType.ZoomValue, RemoteValueMode.ReadOnly));
        }
    }
}
