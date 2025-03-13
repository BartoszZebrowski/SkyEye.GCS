using SkyEye.Connector.MessagesService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEye.Connector.Datalink
{
    public interface IRemoteValue
    {
        RemoteValueType RemoteValueType { get; init; }
        RemoteValueMode RemoteValueMode { get; init; }
        public bool ToUpdate { get; set; }
    }
}
