using ForTheKing.Model;

namespace ForTheKing.Persistence;

public class GameBoard
{
	public const byte MAPRADIUS = 7;
	public const byte MAPLENGTH = MAPRADIUS * 2 + 1;
	public const uint TICKTIME = 300;

	private readonly GameModel game;
	private readonly Tile?[,] map;
	private readonly List<Tile> tiles;
	private Castle castle;
	private uint gold;
	private uint timer;

	public List<Tile> Tiles => tiles;
	public uint Gold => gold;
	public uint Timer => timer;
	public Tile? this[int x, int y]
	{
		get => map[x + MAPRADIUS, y + MAPRADIUS];
		set => map[x + MAPRADIUS, y + MAPRADIUS] = value;
	}

	public GameBoard(GameModel game)
	{
		this.game = game;

		map = new Tile?[MAPLENGTH, MAPLENGTH];
		tiles = [];

		gold = 10;

		castle = new(game, new Coordinate(0, 0));
		this[castle.Position.X, castle.Position.Y] = castle;
		tiles.Add(castle);
	}

	public void Clear()
	{
		// Clearing board
		for (int i = 0; i < MAPLENGTH; ++i)
		{
			for (int j = 0; j < MAPLENGTH; ++j)
				map[j, i] = null;
		}
		tiles.Clear();

		// Initial state
		gold = 10;
		timer = 0;

		castle = new Castle(game, new Coordinate(0, 0));
		this[castle.Position.X, castle.Position.Y] = castle;
		tiles.Add(castle);
	}

	public Tile? GetTile(Coordinate c)
	{
		return -MAPRADIUS <= c.X && c.X <= MAPRADIUS && -MAPRADIUS <= c.Y && c.Y <= MAPRADIUS ? this[c.X, c.Y] : null;
	}

	public bool AddTile(Tile tile)
	{
		if (this[tile.Position.X, tile.Position.Y] is not null)
			return false;

		this[tile.Position.X, tile.Position.Y] = tile;
		tiles.Add(tile);

		return true;
	}

	public void Move(Coordinate old, Coordinate current)
	{
		Tile? tile = this[old.X, old.Y];
		this[old.X, old.Y] = null;
		this[current.X, current.Y] = tile;
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

	public void AdvanceTimer()
	{
		++timer;
	}

	public void ChangeGold(int difference)
	{
		gold = (uint)(gold + difference);
	}

	public override string ToString()
	{
		string result = string.Empty;

		for (int i = 0; i < MAPLENGTH; ++i)
		{
			result += $"{(byte?)map[0, i]?.Type() ?? 0}";
			for (int j = 1; j < MAPLENGTH; ++j)
				result += $", {(byte?)map[j, i]?.Type() ?? 0}";
			result += "\n";
		}

		return result;
	}
}