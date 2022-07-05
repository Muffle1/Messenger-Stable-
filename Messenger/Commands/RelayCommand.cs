using System;
using System.Windows.Input;

namespace Messenger
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _Execute;
        private readonly Func<object, bool> _CanExecute;
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        internal RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _Execute = execute;
            _CanExecute = canExecute;
        }

        /// <summary>
        /// Метод проверки команды на пустоту
        /// </summary>
        /// <param name="parameter">команда</param>
        /// <returns>Может ли команда выполняться</returns>
        public bool CanExecute(object parameter)
        {
            return _CanExecute == null || _CanExecute(parameter);
        }

        /// <summary>
        /// Выполнение команды
        /// </summary>
        /// <param name="parameter">комманда</param>
        public void Execute(object parameter)
        {
            _Execute(parameter);
        }
    }
}
