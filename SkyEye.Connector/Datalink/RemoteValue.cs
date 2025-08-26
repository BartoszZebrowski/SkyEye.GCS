using SkyEye.Connector.CommandService;
using SkyEye.Connector.Datalink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEye.Connector.MessagesService
{
    /// <summary>
    /// Klasa reprezentująca pojedynczą zdalną wartość określonego typu.
    /// Umożliwia subskrypcję zmian oraz zarządzanie procesem aktualizacji wartości.
    /// </summary>
    /// <typeparam name="T">Typ przechowywanej wartości.</typeparam>
    public class RemoteValue<T> : IRemoteValue
    {
        /// <summary>
        /// Zdarzenie wywoływane w momencie zmiany wartości.
        /// </summary>
        public event Action<T> ValueChanged;

        /// <summary>
        /// Typ zdalnej wartości.
        /// </summary>
        public RemoteValueType RemoteValueType { get; init; }

        private bool _toUpdate = false;

        /// <summary>
        /// Flaga określająca, czy wartość oczekuje na aktualizację.
        /// </summary>
        public bool ToUpdate
        {
            get => _toUpdate;
            set => _toUpdate = value;
        }

        private T _value;
        private T _valueToUpdate;

        /// <summary>
        /// Aktualna wartość. 
        /// </summary>
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

        /// <summary>
        /// Konstruktor inicjalizujący nową zdalną wartość o określonym typie.
        /// </summary>
        /// <param name="remoteValueType">Typ zdalnej wartości.</param>
        public RemoteValue(RemoteValueType remoteValueType)
        {
            RemoteValueType = remoteValueType;
        }

        /// <summary>
        /// Aktualizuje przechowywaną wartość i wywołuje zdarzenie zmiany.
        /// </summary>
        /// <param name="value">Nowa wartość do ustawienia.</param>
        internal void UpdateValue(T value)
        {
            lock (this)
            {
                _value = value;
                ValueChanged?.Invoke(_value);
            }
        }

        /// <summary>
        /// Zwraca wartość oczekującą na aktualizację.
        /// </summary>
        /// <returns>Wartość do zaktualizowania.</returns>
        internal T GetValueToUpdate()
        {
            return _valueToUpdate;
        }
    }
}
