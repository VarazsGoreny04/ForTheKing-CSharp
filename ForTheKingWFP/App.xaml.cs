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
	private MainWindow _menu = null!;
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

		_menu = new MainWindow { DataContext = _viewModel };
		_menu.Closing += new CancelEventHandler(Closing);
		_menu.Show();

		/*_game = new GameWindow { DataContext = _viewModel };
		_game.Closing += new CancelEventHandler(Closing);
		_game.KeyDown += InputConverter;*/


		//_timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.4d) };
		//_timer.Tick += new EventHandler(OneStep);
	}

	private void NewGame(object? sender, EventArgs e)
	{
		//uint tableSize = (uint)Math.Round(_menu.sliderTableSize.Value);

		_model.OnCreatingGame();

		//ShowMenu(false);
		//_menu.buttonResume.IsEnabled = true;

		SetWindowSize(GameViewModel.TableSize);
		//_GamePhase = GamePhase.Hold;
	}

	private void Closing(object? sender, CancelEventArgs e)
	{
		//bool restartTimer = _timer.IsEnabled;

		//_timer.Stop();

		if (MessageBox.Show("Are you sure you want to exit?", "ForTheKing", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
		{
			e.Cancel = true;

			/*if (restartTimer)
				_timer.Start();*/
		}
		else
		{
			_model.End();

			_menu.Closing -= new CancelEventHandler(Closing);
			//_game.Closing -= new CancelEventHandler(Closing);
			try
			{
				_menu.Close();
			}
			catch { }
			/*try
			{
				_game.Close();
			}
			catch { }*/
		}
	}

	private void SetWindowSize(uint tableSize)
	{
		uint size = tableSize * BLOCK;

		_menu.MaxHeight = size + 60;
		_menu.MinHeight = size + 60;

		_menu.MaxWidth = size;
		_menu.MinWidth = size;
	}

	/*private void ShowMenu(bool onOff)
	{
		if (onOff)
		{
			_menu.Show();
			_game.Hide();
		}
		else
		{
			_menu.Hide();
			_game.Show();
		}
	}*/
}