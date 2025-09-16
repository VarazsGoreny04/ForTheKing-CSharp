using ForTheKing;
using ForTheKingWFP.ViewModel;
using ForTheKingWFP.View;
using System.ComponentModel;
using System.Windows;

namespace ForTheKingWFP;

public partial class App : Application
{
	private const int BLOCK = 20;

	private GameModel _model = null!;
	private GameViewModel _viewModel = null!;
	private GameWindow _game = null!;
	//private GameWindow _game = null!;
	//private DispatcherTimer _timer = null!;

	public App()
	{
		Startup += new StartupEventHandler(AppStartup);
	}

	public void AppStartup(object? sender, StartupEventArgs e)
	{
		_model = new GameModel();

		_viewModel = new GameViewModel(_model);
		_viewModel.NewGame += new EventHandler(NewGame);

		_game = new GameWindow { DataContext = _viewModel };
		_game.Closing += new CancelEventHandler(Closing);
		_game.Show();
	}

	private void NewGame(object? sender, EventArgs e)
	{
		_model.OnCreatingGame();

		SetWindowSize(GameViewModel.TableSize);
	}

	private void Closing(object? sender, CancelEventArgs e)
	{
		if (MessageBox.Show("Are you sure you want to exit?", "ForTheKing", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
			e.Cancel = true;
		else
		{
			_model.End();

			_game.Closing -= new CancelEventHandler(Closing);
			try
			{
				_game.Close();
			}
			catch { }
		}
	}

	private void SetWindowSize(uint tableSize)
	{
		uint size = tableSize * BLOCK;

		_game.MaxHeight = size + 60;
		_game.MinHeight = size + 60;

		_game.MaxWidth = size;
		_game.MinWidth = size;
	}
}