using System.Collections.ObjectModel;
using System.Diagnostics;
using ForTheKing;

namespace ForTheKingWFP.ViewModel;

public class Model : ViewModelBase
{
	private readonly GameModel _model;

	public static byte TableSize => GameModel.MAPLENGTH;
	public uint Timer => _model.Timer;
	public uint Gold => _model.Gold;
	//public GamePhase GamePhase { get; set; }
	public Field[,] Fields { get; set; }
	public ObservableCollection<Field> ObservableFields { get; set; }

	public DelegateCommand NewGameCommand { get; private set; }
	public DelegateCommand ResumeCommand { get; private set; }
	public DelegateCommand PauseCommand { get; private set; }

	private EventHandler? _newGame;
	private EventHandler? _resume;
	private EventHandler? _pause;

	public EventHandler? NewGame { get => _newGame; set => _newGame = value; }
	public EventHandler? Resume { get => _resume; set => _resume = value; }
	public EventHandler? Pause { get => _pause; set => _pause = value; }

	public Model(GameModel model)
	{
		_model = model;
		_model.CreatingGame += new EventHandler<Tile.InitializeEventArgs>(InitializeGame);
		_model.PlacingTile += new EventHandler<Tile.PlaceEventArgs>(FieldAdded);
		_model.Moving += new EventHandler<Tile.MoveEventArgs>(FieldMoved);

		NewGameCommand = new DelegateCommand(param => OnNewGame());
		ResumeCommand = new DelegateCommand(param => OnResume());
		PauseCommand = new DelegateCommand(param => OnPause());

		Fields = null!;
		ObservableFields = null!;

		//GamePhase = GamePhase.Pause;
	}

	public void InitializeGame(object? sender, Tile.InitializeEventArgs e)
	{
		ObservableFields = [];
		Fields = new Field[TableSize, TableSize];

		for (sbyte i = 0; i < TableSize; ++i)
		{
			for (sbyte j = 0; j < TableSize; ++j)
			{
				Field oneField = new(j, i);
				ObservableFields.Add(oneField);
				Fields[j, i] = oneField;
			}
		}

		Debug.Write("\n\n\n\ncica\n\n\n\n");

		foreach (Tile tile in _model.Tiles)
			Fields[tile.Position.Y + GameModel.MAPRADIUS, tile.Position.X + GameModel.MAPRADIUS].Value = tile.Type();

		//OnPropertyChanged(nameof(Score));
		OnPropertyChanged(nameof(TableSize));
		OnPropertyChanged(nameof(ObservableFields));
	}

	/*public void ScoreAdvanced(SnakeEventArgs e)
	{
		OnPropertyChanged(nameof(Score));

		if (e.EmptyField)
			Fields[_model.Table.Apple.X, _model.Table.Apple.Y].Value = FieldNames.Apple;
	}*/

	public void FieldAdded(object? sender, Tile.PlaceEventArgs e)
	{
		(int x, int y) = (e.Position.X + GameModel.MAPRADIUS, e.Position.Y + GameModel.MAPRADIUS);

		Fields[x, y].Value = e.Field;
	}

	public void FieldMoved(object? sender, Tile.MoveEventArgs e)
	{
		(int oldX, int oldY)  = (e.OldPosition.X + GameModel.MAPRADIUS, e.OldPosition.Y + GameModel.MAPRADIUS);
		(int newX, int newY)  = (e.NewPosition.X + GameModel.MAPRADIUS, e.NewPosition.Y + GameModel.MAPRADIUS);

		if (Fields[oldX, oldY].Value is FieldNames.Empty)
			return;

		Fields[newX, newY].Value = Fields[oldX, oldY].Value;
		Fields[oldX, oldY].Value = FieldNames.Empty;
	}

	private void OnNewGame()
	{
		_newGame?.Invoke(this, EventArgs.Empty);
	}

	private void OnResume()
	{
		_resume?.Invoke(this, EventArgs.Empty);
	}

	private void OnPause()
	{
		_pause?.Invoke(this, EventArgs.Empty);
	}
}
