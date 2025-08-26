using SkyEye.Connector.Datalink;
using SkyEye.Connector.MessagesService;
using SkyEye.Connector.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SkyEye.UI.Commands
{
    /// <summary>
    /// Komenda zmieniająca tryb pracy systemu.
    /// Implementuje interfejs <see cref="ICommand"/> i umożliwia przełączanie między trybem ręcznym a trybem podążania.
    /// </summary>
    public class ChangeModeCommand : ICommand
    {
        /// <summary>
        /// Zdarzenie sygnalizujące zmianę stanu możliwości wykonania komendy.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        private RemoteValue<int> _workingModeRemoteValue;

        /// <summary>
        /// Konstruktor komendy. Inicjalizuje dostęp do zdalnej wartości odpowiadającej trybowi pracy.
        /// </summary>
        /// <param name="datalink">Obiekt datalink z dostępem do zdalnych wartości.</param>
        public ChangeModeCommand(Datalink datalink)
        {
            _workingModeRemoteValue = datalink.GetRemoteValue<int>(RemoteValueType.WorkingMode);
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
        /// Wykonuje zmianę trybu pracy systemu na podstawie podanego parametru.
        /// </summary>
        /// <param name="parameter">Nazwa trybu pracy: "ManualMode" lub "FollowMode".</param>
        public async void Execute(object? parameter)
        {
            switch ((string)parameter)
            {
                case "ManualMode":
                    _workingModeRemoteValue.Value = (int)WorkingMode.ManualMode;
                    break;
                case "FollowMode":
                    _workingModeRemoteValue.Value = (int)WorkingMode.FollowMode;
                    break;
            }
        }
    }

}
