using SkyEye.Connector.CommandService;
using SkyEye.Connector.Datalink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEye.Connector.MessagesService
{
    public class RemoteValue<T> : IRemoteValue
    {
        public event Action<T> ValueChanged;

        public RemoteValueType RemoteValueType { get; init; }

        private bool _toUpdate = false;
        public bool ToUpdate 
        { 
            get => _toUpdate; 
            set => _toUpdate = value; 
        } 

        private T _value;
        private T _valueToUpdate;

        public T Value
        {
            get
            {
                return _value;
            }
            set
            {
                _valueToUpdate = value;
                ToUpdate = true;
            }
        }

        public RemoteValue(RemoteValueType remoteValueType)
        {
            RemoteValueType = remoteValueType;
        }

        internal void UpdateValue(T value)
        {
            lock (this)
            {
                _value = value;
                ValueChanged?.Invoke(_value);
            }
        }

        internal T GetValueToUpdate()
        {
            return _valueToUpdate;
        }
    }
}
