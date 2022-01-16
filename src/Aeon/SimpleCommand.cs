using System;
using System.Windows.Input;

namespace Aeon.Emulator.Launcher
{
    /// <summary>
    /// Implements ICommand using arbitrary delegates.
    /// </summary>
    internal sealed class SimpleCommand : ICommand
    {
        private readonly Func<bool> canExecute;
        private readonly Action execute;
        private bool canExecuteState;

        /// <summary>
        /// Initializes a new instance of the SimpleCommand class.
        /// </summary>
        /// <param name="canExecute">Delegate invoked to determine whether the command can execute.</param>
        /// <param name="execute">Delegate invoked to execute the command.</param>
        public SimpleCommand(Func<bool> canExecute, Action execute)
        {
            this.canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecuteState = canExecute();
        }

        /// <summary>
        /// Occurs when the return value of the CanExecute method has changed.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Returns a value indicating whether the command can currently execute.
        /// </summary>
        /// <param name="parameter">Parameter for the command.</param>
        /// <returns>Value indicating whether the command can currently execute.</returns>
        public bool CanExecute(object parameter) => this.canExecuteState;
        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">Parameter for the command.</param>
        public void Execute(object parameter) => this.execute();
        /// <summary>
        /// Updates the CanExecute return value if necessary.
        /// </summary>
        public void UpdateState()
        {
            bool newState = this.canExecute();
            if (this.canExecuteState != newState)
            {
                this.canExecuteState = newState;
                OnCanExecuteChanged(EventArgs.Empty);
            }
        }

        private void OnCanExecuteChanged(EventArgs e) => this.CanExecuteChanged?.Invoke(this, e);
    }
}
