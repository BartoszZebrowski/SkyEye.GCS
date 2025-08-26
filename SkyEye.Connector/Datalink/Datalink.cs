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
    /// <summary>
    /// Klasa odpowiedzialna za przechowywanie i udostępnianie zdalnych wartości (RemoteValues).
    /// </summary>
    public class Datalink
    {
        /// <summary>
        /// Kolekcja wszystkich zdalnych wartości dostępnych w systemie.
        /// </summary>
        public List<IRemoteValue> RemoteValues { get; private set; } = new();

        /// <summary>
        /// Konstruktor klasy Datalink. Tworzy i inicjalizuje listę zdalnych wartości.
        /// </summary>
        public Datalink()
        {
            CreateRemoteValues();
        }

        /// <summary>
        /// Zwraca zdalną wartość o określonym typie.
        /// </summary>
        /// <typeparam name="T">Typ wartości.</typeparam>
        /// <param name="remoteValueType">Typ zdalnej wartości do pobrania.</param>
        /// <returns>Obiekt RemoteValue określonego typu.</returns>
        public RemoteValue<T> GetRemoteValue<T>(RemoteValueType remoteValueType)
        {
            return (RemoteValue<T>)RemoteValues
                .Where(remoteValue => remoteValue.RemoteValueType == remoteValueType)
                .First();
        }

        /// <summary>
        /// Tworzy wszystkie dostępne zdalne wartości i dodaje je do kolekcji.
        /// </summary>
        private void CreateRemoteValues()
        {
            RemoteValues.Add(new RemoteValue<int>(RemoteValueType.Ping));
            RemoteValues.Add(new RemoteValue<int>(RemoteValueType.WorkingMode));
            RemoteValues.Add(new RemoteValue<float>(RemoteValueType.TargetHorizontalAngle));
            RemoteValues.Add(new RemoteValue<float>(RemoteValueType.TargetVerticalAngle));
            RemoteValues.Add(new RemoteValue<float>(RemoteValueType.ActualHorizontaAngle));
            RemoteValues.Add(new RemoteValue<float>(RemoteValueType.ActualVerticalAngle));
            RemoteValues.Add(new RemoteValue<float>(RemoteValueType.ZoomValue));
        }
    }

}
