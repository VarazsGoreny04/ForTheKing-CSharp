using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ForTheKingWFP.ViewModel;

public abstract partial class ViewModelBase : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	protected ViewModelBase() { }

	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}