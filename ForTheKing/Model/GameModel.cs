using ForTheKing.Persistence;

namespace ForTheKing.Model;

public class GameModel
{
	public class TimerEventArgs : EventArgs { }
	public class GoldEventArgs : EventArgs { }

	private readonly GameBoard board;

	private (Task Task, CancellationTokenSource TokenSource) gameLoop;
	private readonly List<CancellationTokenSource> tasks;

	public GameBoard Board => board;
	public List<Tile> Tiles => board.Tiles;
	public uint Gold => board.Gold;
	public uint Timer => board.Timer;

	public Tile? this[int x, int y] { get => board[x, y]; private set => board[x, y] = value; }

	public event EventHandler<Tile.InitializeEventArgs>? CreatingGame;
	public event EventHandler<TimerEventArgs>? TimerTicking;
	public event EventHandler<GoldEventArgs>? GoldChanging;
	public event EventHandler<Tile.PlaceEventArgs>? PlacingTile;
	public event EventHandler<Tile.MoveEventArgs>? Moving;
	public event EventHandler<Tile.DieEventArgs>? Dying;

	public GameModel()
	{
		tasks = [];

		board = new GameBoard(this);

		gameLoop = (null!, new CancellationTokenSource());
	}

	public Task Initialize()
	{
		board.Clear();

		gameLoop.TokenSource = new CancellationTokenSource();
		gameLoop.Task = Task.Run(() => GameLoop(gameLoop.TokenSource), gameLoop.TokenSource.Token);

		return Task.CompletedTask;
	}

	public async Task GameLoop(CancellationTokenSource token)
	{
		Random rnd = new();

		while (!token.IsCancellationRequested)
		{
			List<Coordinate> possibilities = [];
			await Task.Delay((int)GameBoard.TICKTIME);

			for (sbyte i = -GameBoard.MAPRADIUS; i <= GameBoard.MAPRADIUS; ++i)
			{
				possibilities.Add(new Coordinate(-GameBoard.MAPRADIUS, i));
				possibilities.Add(new Coordinate((sbyte)GameBoard.MAPRADIUS, i));
				possibilities.Add(new Coordinate(i, -GameBoard.MAPRADIUS));
				possibilities.Add(new Coordinate(i, (sbyte)GameBoard.MAPRADIUS));
			}

			for (int i = 0; board.Timer % 5 == 0 && i < Math.Min(board.Timer / 100 + 1, 6); ++i)
			{
				int index = rnd.Next(possibilities.Count);

				AddTile(new Goblin(this, possibilities[index]));

				possibilities.RemoveAt(index);
			}

			OnGoldAdvanced(1);
			OnTimerAdvanced();
		}
	}

	public Task End()
	{
		gameLoop.TokenSource.Cancel();
		tasks.ForEach(x => x.Cancel());
		tasks.Clear();

		return Task.CompletedTask;
	}

	public bool Buy(Ally tile)
	{
		if (tile.Cost() > board.Gold || !AddTile(tile))
			return false;

		OnGoldAdvanced(-tile.Cost());

		return true;
	}

	private bool AddTile(Tile tile)
	{
		if (!board.AddTile(tile))
			return false;

		CancellationTokenSource newSource = new();
		Task.Run(() => tile.Run(newSource), newSource.Token);
		tasks.Add(newSource);

		return true;
	}

	public async void OnCreatingGame()
	{
		await End();

		await Initialize();

		CreatingGame?.Invoke(null, new Tile.InitializeEventArgs());
	}

	public void OnTimerAdvanced()
	{
		board.AdvanceTimer();

		TimerTicking?.Invoke(null, new TimerEventArgs());
	}

	public void OnGoldAdvanced(int difference)
	{
		board.ChangeGold(difference);

		GoldChanging?.Invoke(null, new GoldEventArgs());
	}

	public void OnPlacingTile(Coordinate position, FieldNames field)
	{
		PlacingTile?.Invoke(null, new Tile.PlaceEventArgs(position, field));
	}

	public void OnMoving(Coordinate old, Coordinate current)
	{
		board.Move(old, current);

		Moving?.Invoke(null, new Tile.MoveEventArgs(old, current));
	}

	public void OnDying(Coordinate position)
	{
		board[position.X, position.Y] = null;

		Dying?.Invoke(null, new Tile.DieEventArgs(position));
	}
}