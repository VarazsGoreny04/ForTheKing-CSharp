using ForTheKing;
using ForTheKingWFP.View;
using ForTheKingWFP.ViewModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace ForTheKingWFP;

public partial class App : Application
{
	private const int BLOCK = 20;

	private GameModel _model = null!;
	private Model _viewModel = null!;
	private MainWindow _menu = null!;
	//private GameWindow _game = null!;
	private DispatcherTimer _timer = null!;

	public App()
	{
		Startup += new StartupEventHandler(AppStartup);
	}

	public void AppStartup(object? sender, StartupEventArgs e)
	{
		_model = new GameModel();

		_viewModel = new Model(_model);
		_viewModel.NewGame += new EventHandler(NewGame);
		_viewModel.Resume += new EventHandler(Resume);
		_viewModel.Pause += new EventHandler(Pause);

		_menu = new MainWindow { DataContext = _viewModel };
		_menu.Closing += new CancelEventHandler(Closing);
		_menu.Show();

		/*_game = new GameWindow { DataContext = _viewModel };
		_game.Closing += new CancelEventHandler(Closing);
		_game.KeyDown += InputConverter;*/


		_timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.4d) };
		//_timer.Tick += new EventHandler(OneStep);
	}

	private void NewGame(object? sender, EventArgs e)
	{
		//uint tableSize = (uint)Math.Round(_menu.sliderTableSize.Value);

		_model.OnCreatingGame();

		//ShowMenu(false);
		//_menu.buttonResume.IsEnabled = true;

		SetWindowSize(Model.TableSize);
		//_viewModel.GamePhase = GamePhase.Hold;
	}

	private void Resume(object? sender, EventArgs e)
	{
		//ShowMenu(false);

		//_viewModel.GamePhase = GamePhase.Start;
		_timer.Start();
	}

	private void Pause(object? sender, EventArgs e)
	{
		_timer.Stop();
		//_viewModel.GamePhase = GamePhase.Pause;

		//ShowMenu(true);
		//_menu.labelTitle.Foreground = Brushes.Black;
		//_menu.labelTitle.Content = "Snake";
	}

	private void Closing(object? sender, CancelEventArgs e)
	{
		bool restartTimer = _timer.IsEnabled;

		_timer.Stop();

		if (MessageBox.Show("Are you sure you want to exit?", "Snake", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
		{
			e.Cancel = true;

			if (restartTimer)
				_timer.Start();
		}
		else
		{
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

		_menu.MaxWidth = size + 20;
		_menu.MinWidth = size + 20;

		_menu.Height = size + 60;
		_menu.Width = size + 20;
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