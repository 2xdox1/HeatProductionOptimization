using System;
using System.Windows.Input;

namespace HeatOptimizerApp.Utils
{
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
        public void Execute(object? parameter) => _execute();

        #pragma warning disable CS0067 // Event is never used
        public event EventHandler? CanExecuteChanged;
    }
}