namespace ForTheKing;

public class GameModel
{
	public class TimerEventArgs : EventArgs { }
	public class GoldEventArgs : EventArgs { }

	public const byte MAPRADIUS = 7;
	public const byte MAPLENGTH = MAPRADIUS * 2 + 1;
	public const uint TICKTIME = 300;

	private uint timer;
	private readonly Tile?[,] map;
	private readonly List<Tile> tiles;
	private Castle castle;
	private uint gold;

	private (Task Task, CancellationTokenSource TokenSource) gameLoop;
	private readonly List<CancellationTokenSource> tasks;

	public List<Tile> Tiles => tiles;
	public uint Gold { get => gold; }
	public uint Timer => timer;
	public Tile? this[int x, int y] { get => map[x + MAPRADIUS, y + MAPRADIUS]; private set => map[x + MAPRADIUS, y + MAPRADIUS] = value; }

	public event EventHandler<Tile.InitializeEventArgs>? CreatingGame;
	public event EventHandler<TimerEventArgs>? TimerTicking;
	public event EventHandler<GoldEventArgs>? GoldChanging;
	public event EventHandler<Tile.PlaceEventArgs>? PlacingTile;
	public event EventHandler<Tile.MoveEventArgs>? Moving;
	public event EventHandler<Tile.DieEventArgs>? Dying;

	public GameModel()
	{
		tasks = [];

		map = new Tile?[MAPLENGTH, MAPLENGTH];
		tiles = [];

		gold = 10;
		timer = 0;

		castle = new(this, new Coordinate(0, 0));
		this[castle.Position.X, castle.Position.Y] = castle;
		tiles.Add(castle);

		gameLoop = (null!, new CancellationTokenSource());
	}

	public Task Initialize()
	{
		// Clearing map
		for (int i = 0; i < MAPLENGTH; ++i)
		{
			for (int j = 0; j < MAPLENGTH; ++j)
				map[j, i] = null;
		}
		tiles.Clear();

		// Initial state
		gold = 10;
		timer = 0;
		castle = new Castle(this, new Coordinate(0, 0));

		this[castle.Position.X, castle.Position.Y] = castle;
		tiles.Add(castle);
		// Game phase => start

		gameLoop.TokenSource = new CancellationTokenSource();
		gameLoop.Task = Task.Run(GameLoop, gameLoop.TokenSource.Token);

		return Task.CompletedTask;
	}

	public async Task GameLoop()
	{
		Random rnd = new();

		while (!gameLoop.TokenSource.IsCancellationRequested)
		{
			List<Coordinate> possibilities = [];
			await Task.Delay((int)TICKTIME);

			for (sbyte i = -MAPRADIUS; i <= MAPRADIUS; ++i)
			{
				possibilities.Add(new Coordinate(-MAPRADIUS, i));
				possibilities.Add(new Coordinate((sbyte)MAPRADIUS, i));
				possibilities.Add(new Coordinate(i, -MAPRADIUS));
				possibilities.Add(new Coordinate(i, (sbyte)MAPRADIUS));
			}

			for (int i = 0; timer % 5 == 0 && i < Math.Min((timer / 100) + 1, 6); ++i)
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

		// Game phase => end

		return Task.CompletedTask;
	}

	public Tile? GetTile(Coordinate c)
	{
		return (-MAPRADIUS <= c.X && c.X <= MAPRADIUS && -MAPRADIUS <= c.Y && c.Y <= MAPRADIUS) ? this[c.X, c.Y] : null;
	}

	public bool Buy(Ally tile)
	{
		if (tile.Cost() > gold || !AddTile(tile))
			return false;

		OnGoldAdvanced(-tile.Cost());

		return true;
	}

	private bool AddTile(Tile tile)
	{
		ref Tile? temp = ref map[tile.Position.X + MAPRADIUS, tile.Position.Y + MAPRADIUS];

		if (temp is not null)
			return false;

		temp = tile;
		tiles.Add(tile);

		CancellationTokenSource newSource = new();
		Task.Run(() => tile.Run(newSource), newSource.Token);
		tasks.Add(newSource);

		return true;
	}

	public List<Tile> GetCircleArea(Tile origin) => GetBoxArea(origin).FindAll(x => Coordinate.DistanceRoundDown(origin.Position, x.Position) <= origin.Range);

	public List<Tile> GetBoxArea(Tile origin)
	{
		List<Tile> result = [];

		for (int i = -origin.Range; i <= origin.Range; ++i)
		{
			for (int j = -origin.Range; j <= origin.Range; ++j)
			{
				if ((j != 0 || i != 0) && GetTile(new Coordinate((sbyte)(origin.Position.X + j), (sbyte)(origin.Position.Y + i))) is Tile current)
					result.Add(current);
			}
		}

		return result;
	}

	public List<Tile> GetPlusArea(Tile origin)
	{
		List<Tile> result = [];

		for (int i = 1; i <= origin.Range; ++i)
		{
			if (GetTile(new Coordinate(origin.Position.X, (sbyte)(origin.Position.Y - i))) is Tile current1)
				result.Add(current1);
			if (GetTile(new Coordinate(origin.Position.X, (sbyte)(origin.Position.Y + i))) is Tile current2)
				result.Add(current2);
			if (GetTile(new Coordinate((sbyte)(origin.Position.X - i), origin.Position.Y)) is Tile current3)
				result.Add(current3);
			if (GetTile(new Coordinate((sbyte)(origin.Position.X + i), origin.Position.Y)) is Tile current4)
				result.Add(current4);
		}

		return result;
	}

	public List<Tile> GetCrossArea(Tile origin)
	{
		List<Tile> result = [];

		for (int i = 1; i <= origin.Range; ++i)
		{
			if (GetTile(new Coordinate((sbyte)(origin.Position.X - i), (sbyte)(origin.Position.Y - i))) is Tile current1)
				result.Add(current1);
			if (GetTile(new Coordinate((sbyte)(origin.Position.X + i), (sbyte)(origin.Position.Y + i))) is Tile current2)
				result.Add(current2);
			if (GetTile(new Coordinate((sbyte)(origin.Position.X - i), (sbyte)(origin.Position.Y - i))) is Tile current3)
				result.Add(current3);
			if (GetTile(new Coordinate((sbyte)(origin.Position.X + i), (sbyte)(origin.Position.Y + i))) is Tile current4)
				result.Add(current4);
		}

		return result;
	}

	public async void OnCreatingGame()
	{
		await Initialize();

		CreatingGame?.Invoke(null, new Tile.InitializeEventArgs());
	}

	public void OnTimerAdvanced()
	{
		++timer;

		TimerTicking?.Invoke(null, new TimerEventArgs());
	}

	public void OnGoldAdvanced(int difference)
	{
		gold = (uint)(gold + difference);

		GoldChanging?.Invoke(null, new GoldEventArgs());
	}

	public void OnPlacingTile(Coordinate position, FieldNames field)
	{
		PlacingTile?.Invoke(null, new Tile.PlaceEventArgs(position, field));
	}

	public void OnMoving(Coordinate old, Coordinate current)
	{
		Tile? tile = this[old.X, old.Y];
		this[old.X, old.Y] = null;
		this[current.X, current.Y] = tile;

		Moving?.Invoke(null, new Tile.MoveEventArgs(old, current));
	}

	public void OnDying(Coordinate position)
	{
		this[position.X, position.Y] = null;

		Dying?.Invoke(null, new Tile.DieEventArgs(position));
	}

	public override string ToString()
	{
		string result = string.Empty;

		for (int i = 0; i < MAPLENGTH; ++i)
		{
			result += $"{((byte?)map[0, i]?.Type()) ?? 0}";
			for (int j = 1; j < MAPLENGTH; ++j)
				result += $", {((byte?)map[j, i]?.Type()) ?? 0}";
			result += "\n";
		}

		return result;
	}
}