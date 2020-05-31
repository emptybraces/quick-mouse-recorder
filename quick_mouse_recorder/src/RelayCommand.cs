using System;
using System.Windows.Input;

namespace quick_mouse_recorder
{
	public class RelayCommand : ICommand
	{
		Action<object> execute;
		Func<object, bool> canExecute;

		public event EventHandler CanExecuteChanged
			{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
		{
			this.execute = execute;
			this.canExecute = canExecute;
		}

		public bool CanExecute(object parameter)
		{
			return this.canExecute == null || this.canExecute(parameter);
		}

		public void Execute(object parameter)
		{
			this.execute(parameter);
		}
	}
}
