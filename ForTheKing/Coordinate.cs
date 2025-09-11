namespace ForTheKing;

public class Coordinate
{
	private byte x;
	private byte y;

	public byte X { get => x; set => x = value; }
	public byte Y { get => y; set => y = value; }

	public Coordinate(byte x, byte y)
	{
		this.x = x;
		this.y = y;
	}

	private static double Distance(Coordinate a, Coordinate b) => Math.Sqrt(Math.Pow(a.x - b.x, 2d) + Math.Pow(a.y - b.y, 2d));

	public static byte DistanceRoundDown(Coordinate a, Coordinate b) => (byte)Distance(a, b);
	public static byte DistanceRoundUp(Coordinate a, Coordinate b) => (byte)Math.Ceiling(Distance(a, b));

	public static byte DistanceBox(Coordinate a, Coordinate b) => (byte)Math.Max(Math.Abs(a.x - b.x), Math.Abs(a.y - b.y));
}