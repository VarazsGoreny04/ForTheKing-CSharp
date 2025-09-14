namespace ForTheKing;

public class GameModel
{
	public const byte MAPRADIUS = 12;
	public const byte MAPLENGTH = MAPRADIUS * 2 + 1;
	public const uint TICKTIME = 200;

	// private static Phase gamePhase;
	// private static Timer timer = new();
	private readonly Tile?[,] map;
	private readonly List<Tile> tiles;
	private uint gold;

	private readonly List<CancellationTokenSource> tasks;

	public Tile?[,] Map => map;
	public List<Tile> Tiles => tiles;
	public uint Gold { get => gold; }

	public event EventHandler<Tile.InitializeEventArgs>? CreatingGame;
	public event EventHandler<Tile.DieEventArgs>? Dying;
	public event EventHandler<Tile.MoveEventArgs>? Moving;

	public GameModel()
	{
		tasks = [];

		map = new Tile?[MAPLENGTH, MAPLENGTH];
		tiles = [];

		gold = 10;

		Castle castle = new(this, new Coordinate(0, 0));
		map[castle.Position.X, castle.Position.Y] = castle;
		tiles.Add(castle);
	}

	public async Task InitializeAsync()
	{
		// Clearing tasks
		tasks.ForEach(x => x.Cancel());
		tasks.Clear();

		// Clearing map
		for (int i = 0; i < MAPLENGTH; ++i)
		{
			for (int j = 0; j < MAPLENGTH; ++j)
				map[j, i] = null;
		}
		tiles.Clear();

		// Initial state
		gold = 10;
		await AddTileAsync(new Castle(this, new Coordinate(0, 0)));
		// Game phase => start
	}

	public Task EndAsync()
	{
		tasks.ForEach(x => x.Cancel());

		// Game phase => end

		return Task.CompletedTask;
	}

	public Tile? GetTile(Coordinate c)
	{
		int mapLength = map.GetLength(0);
		int x = c.X + MAPRADIUS;
		int y = c.Y + MAPRADIUS;

		return (0 <= x && x < mapLength && 0 < y && y < mapLength) ? map[x, y] : null;
	}

	public async Task<bool> BuyAsync(Ally tile)
	{
		Task<bool> getBoolTask = AddTileAsync(tile);

		if (tile.Cost() > gold | !(await getBoolTask))
			return false;

		gold -= tile.Cost();

		return true;
	}

	private async Task<bool> AddTileAsync(Tile tile)
	{
		ref Tile? temp = ref map[tile.Position.X + MAPRADIUS, tile.Position.Y + MAPRADIUS];

		if (temp is not null)
			return false;

		CancellationTokenSource newSource = new();
		Task getTask = Task.Run(tile.Run, newSource.Token);

		temp = tile;
		tiles.Add(tile);
		tasks.Add(newSource);

		await getTask;
		
		return true;
	}

	public List<Tile> GetCircleArea(Tile origin) => GetBoxArea(origin).FindAll(x => Coordinate.DistanceRoundDown(origin.Position, x.Position) < origin.Range);

	public List<Tile> GetBoxArea(Tile origin)
	{
		List<Tile> result = [];

		for (int i = -origin.Range; i <= origin.Range; ++i)
		{
			for (int j = -origin.Range; j <= origin.Range; ++j)
			{
				if (j != 0 && i != 0 && GetTile(new Coordinate((sbyte)(origin.Position.X - j), (sbyte)(origin.Position.Y - i))) is Tile current)
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
		await InitializeAsync();

		CreatingGame?.Invoke(null, new Tile.InitializeEventArgs());
	}

	public void OnMoving(Coordinate old, Coordinate current)
	{
		Tile? tile = map[old.X + MAPRADIUS, old.Y + MAPRADIUS];
		map[old.X + MAPRADIUS, old.Y + MAPRADIUS] = null;
		map[current.X + MAPRADIUS, current.Y + MAPRADIUS] = tile;

		Moving?.Invoke(null, new Tile.MoveEventArgs(old, current));
	}

	public void OnDying(Coordinate position)
	{
		map[position.X, position.Y] = null;

		Dying?.Invoke(null, new Tile.DieEventArgs(position));
	}
}