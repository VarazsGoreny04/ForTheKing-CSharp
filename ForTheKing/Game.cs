using System.Timers;

namespace ForTheKing;

public static class Game
{
	public const byte MAPLENGTH = 12;

	private static Phase gamePhase;
	private static Timer timer = new();
	private static Tile?[,] map = new Tile?[MAPLENGTH * 2 + 1, MAPLENGTH * 2 + 1];
	private static uint gold;

	public static bool Buy(Ally tile)
	{
		if (tile.Cost() > gold || !AddTile(tile))
			return false;

		gold -= tile.Cost();
		return true;
	}

	private static bool AddTile(Tile tile)
	{
		ref Tile? temp = ref map[tile.Position.X + MAPLENGTH, tile.Position.Y + MAPLENGTH];

		if (temp is not null)
			return false;

		temp = tile;
		return true;
	}

	public static List<Tile> GetCircleArea(Tile origin)
	{
		for (int i = origin)
	}

	public static List<Tile> GetBoxArea(Tile origin)
	{
		for (int i = origin)
	}
}