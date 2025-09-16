using ForTheKing;
using System.Collections.ObjectModel;

namespace ForTheKingWFP.ViewModel;

public class GameViewModel : ViewModelBase
{
	private readonly GameModel _model;

	public static byte TableSize => GameModel.MAPLENGTH;
	public uint Timer => _model.Timer;
	public uint Gold => _model.Gold;
	//public GamePhase GamePhase { get; set; }
	public Field[,] Fields { get; set; }
	public ObservableCollection<Field> ObservableFields { get; set; }

	public DelegateCommand NewGameCommand { get; private set; }

	private EventHandler? _newGame;

	public EventHandler? NewGame { get => _newGame; set => _newGame = value; }

	public GameViewModel(GameModel model)
	{
		_model = model;
		_model.CreatingGame += new EventHandler<Tile.InitializeEventArgs>(InitializeGame);
		_model.TimerTicking += new EventHandler<GameModel.TimerEventArgs>(TimerAdvanced);
		_model.GoldChanging += new EventHandler<GameModel.GoldEventArgs>(GoldAdvanced);
		_model.PlacingTile += new EventHandler<Tile.PlaceEventArgs>(FieldAdded);
		_model.Moving += new EventHandler<Tile.MoveEventArgs>(FieldMoved);
		_model.Dying += new EventHandler<Tile.DieEventArgs>(FieldKilled);

		NewGameCommand = new DelegateCommand(param => OnNewGame());

		Fields = null!;
		ObservableFields = null!;
	}

	public void InitializeGame(object? sender, Tile.InitializeEventArgs e)
	{
		ObservableFields = [];
		Fields = new Field[TableSize, TableSize];

		for (sbyte i = 0; i < TableSize; ++i)
		{
			for (sbyte j = 0; j < TableSize; ++j)
			{
				Field oneField = new(j, i)
				{
					BuyCommand = new DelegateCommand(param =>
					{
						if (param is Coordinate c)
							_model.BuyAsync(new Knight(_model, new((sbyte)(c.X - GameModel.MAPRADIUS), (sbyte)(c.Y - GameModel.MAPRADIUS))));
					})
				};
				ObservableFields.Add(oneField);
				Fields[j, i] = oneField;
			}
		}

		foreach (Tile tile in _model.Tiles)
			Fields[tile.Position.Y + GameModel.MAPRADIUS, tile.Position.X + GameModel.MAPRADIUS].Value = tile.Type();

		OnPropertyChanged(nameof(TableSize));
		OnPropertyChanged(nameof(ObservableFields));
	}

	public void TimerAdvanced(object? sender, GameModel.TimerEventArgs e)
	{
		OnPropertyChanged(nameof(Timer));
	}

	public void GoldAdvanced(object? sender, GameModel.GoldEventArgs e)
	{
		OnPropertyChanged(nameof(Gold));
	}

	public void FieldAdded(object? sender, Tile.PlaceEventArgs e)
	{
		(int x, int y) = (e.Position.X + GameModel.MAPRADIUS, e.Position.Y + GameModel.MAPRADIUS);

		Fields[x, y].Value = e.Field;
	}

	public void FieldMoved(object? sender, Tile.MoveEventArgs e)
	{
		(int oldX, int oldY) = (e.OldPosition.X + GameModel.MAPRADIUS, e.OldPosition.Y + GameModel.MAPRADIUS);
		(int newX, int newY) = (e.NewPosition.X + GameModel.MAPRADIUS, e.NewPosition.Y + GameModel.MAPRADIUS);

		if (Fields[oldX, oldY].Value is FieldNames.Empty)
			throw new NotImplementedException();

		Fields[newX, newY].Value = Fields[oldX, oldY].Value;
		Fields[oldX, oldY].Value = FieldNames.Empty;
	}

	private void FieldKilled(object? sender, Tile.DieEventArgs e)
	{
		(int x, int y) = (e.Position.X + GameModel.MAPRADIUS, e.Position.Y + GameModel.MAPRADIUS);

		Fields[x, y].Value = FieldNames.Empty;
	}

	private void OnNewGame()
	{
		_newGame?.Invoke(this, EventArgs.Empty);
	}
}
