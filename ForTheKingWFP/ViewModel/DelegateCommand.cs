using System.Windows.Input;

namespace ForTheKingWFP.ViewModel;

public class DelegateCommand(Func<object?, bool>? canExecute, Action<object?> execute) : ICommand
{
	private readonly Action<object?> _execute = execute ?? throw new ArgumentNullException(nameof(execute));
	private readonly Func<object?, bool>? _canExecute = canExecute;

	public DelegateCommand(Action<object?> execute) : this(null, execute) { }

	public event EventHandler? CanExecuteChanged;

	public bool CanExecute(object? parameter)
	{
		return _canExecute is null || _canExecute(parameter);
	}

	public void Execute(object? parameter)
	{
		if (!CanExecute(parameter))
			throw new InvalidOperationException("Command execution is disabled.");

		_execute(parameter);
	}

	public void RaiseCanExecuteChanged()
	{
		CanExecuteChanged?.Invoke(this, EventArgs.Empty);
	}
}