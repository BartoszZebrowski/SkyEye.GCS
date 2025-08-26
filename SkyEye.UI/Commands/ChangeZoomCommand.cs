using SkyEye.Connector.Datalink;
using SkyEye.Connector.MessagesService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SkyEye.UI.Commands
{
    /// <summary>
    /// Komenda zmieniająca wartość zoomu kamery.
    /// Implementuje interfejs <see cref="ICommand"/> i umożliwia regulację przybliżenia.
    /// </summary>
    public class ChangeZoomCommand : ICommand
    {
        /// <summary>
        /// Zdarzenie sygnalizujące zmianę stanu możliwości wykonania komendy.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        private RemoteValue<float> _zoomValueRemoteValue;

        /// <summary>
        /// Konstruktor komendy. Inicjalizuje dostęp do zdalnej wartości odpowiadającej zoomowi.
        /// </summary>
        /// <param name="datalink">Obiekt datalink z dostępem do zdalnych wartości.</param>
        public ChangeZoomCommand(Datalink datalink)
        {
            _zoomValueRemoteValue = datalink.GetRemoteValue<float>(RemoteValueType.ZoomValue);
        }

        /// <summary>
        /// Określa, czy komenda może być wykonana.
        /// </summary>
        /// <param name="parameter">Parametr komendy.</param>
        /// <returns>Zawsze zwraca true.</returns>
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        /// <summary>
        /// Wykonuje zmianę zoomu na podstawie przekazanego parametru.
        /// </summary>
        /// <param name="parameter">Wartość zmiany zoomu w formie tekstowej.</param>
        public void Execute(object? parameter)
        {
            if (parameter is null)
                return;

            var zoomValue = float.Parse((string)parameter);

            _zoomValueRemoteValue.Value += zoomValue;
        }
    }

}
