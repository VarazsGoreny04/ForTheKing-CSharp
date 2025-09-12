using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace ForTheKing;

public static class Game
{
	public const byte MAPLENGTH = 12;
	public const uint TICKTIME = 200;

	// private static Phase gamePhase;
	// private static Timer timer = new();
	private static readonly Tile?[,] map = new Tile?[(MAPLENGTH * 2) + 1, (MAPLENGTH * 2) + 1];
	private static readonly List<Tile> tiles = [];
	private static uint gold = 10;

	private static List<CancellationTokenSource> tasks = [];
	public static event EventHandler<Tile.DieEventArgs>? Dying;
	public static event EventHandler<Tile.MoveEventArgs>? Moving;

	public static async Task InitializeAsync()
	{
		// Clearing tasks
		tasks.ForEach(x => x.Cancel());
		tasks.Clear();

		// Clearing map
		for (int i = (MAPLENGTH * 2) + 1; i >= 0; --i)
		{
			for (int j = (MAPLENGTH * 2) + 1; j >= 0; --j)
				map[j, i] = null;
		}
		tiles.Clear();

		// Initial state
		gold = 10;
		await AddTileAsync(new Castle(new Coordinate(0, 0)));
		// Game phase => start
	}

	public static Task EndAsync()
	{
		tasks.ForEach(x => x.Cancel());

		// Game phase => end

		return Task.CompletedTask;
	}

	public static Tile? GetTile(Coordinate c)
	{
		int mapLength = map.GetLength(0);
		int x = c.X + MAPLENGTH;
		int y = c.Y + MAPLENGTH;

		return (0 <= x && x < mapLength && 0 < y && y < mapLength) ? map[x, y] : null;
	}

	public static async Task<bool> BuyAsync(Ally tile)
	{
		if (tile.Cost() > gold || !(await AddTileAsync(tile)))
			return false;

		gold -= tile.Cost();

		return true;
	}

	private static async Task<bool> AddTileAsync(Tile tile)
	{
		ref Tile? temp = ref map[tile.Position.X + MAPLENGTH, tile.Position.Y + MAPLENGTH];

		if (temp is not null)
			return false;

		temp = tile;
		tiles.Add(tile);
		CancellationTokenSource newSource = new();
		await Task.Run(tile.Run, newSource.Token);
		tasks.Add(newSource);
		
		return true;
	}

	public static List<Tile> GetCircleArea(Tile origin) => GetBoxArea(origin).FindAll(x => Coordinate.DistanceRoundDown(origin.Position, x.Position) < origin.Range);

	public static List<Tile> GetBoxArea(Tile origin)
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

	public static List<Tile> GetPlusArea(Tile origin)
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

	public static List<Tile> GetCrossArea(Tile origin)
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

	public static void OnMoving(Coordinate old, Coordinate current)
	{
		Tile? tile = map[old.X + MAPLENGTH, old.Y + MAPLENGTH];
		map[old.X + MAPLENGTH, old.Y + MAPLENGTH] = null;
		map[current.X + MAPLENGTH, current.Y + MAPLENGTH] = tile;

		Moving?.Invoke(null, new Tile.MoveEventArgs(old, current));
	}

	public static void OnDying(Coordinate position)
	{
		map[position.X, position.Y] = null;

		Dying?.Invoke(null, new Tile.DieEventArgs(position));
	}
}