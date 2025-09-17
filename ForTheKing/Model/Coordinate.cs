namespace ForTheKing.Model;

public class Coordinate(sbyte x, sbyte y)
{
	private sbyte x = x;
	private sbyte y = y;

	public sbyte X { get => x; set => x = value; }
	public sbyte Y { get => y; set => y = value; }

	public static double Distance(Coordinate a, Coordinate b) => Math.Round(Math.Sqrt(Math.Pow(a.x - b.x, 2d) + Math.Pow(a.y - b.y, 2d)), 1);

	public static byte DistanceRoundDown(Coordinate a, Coordinate b) => (byte)Distance(a, b);
	public static byte DistanceRoundUp(Coordinate a, Coordinate b) => (byte)Math.Ceiling(Distance(a, b));

	public static byte DistanceBox(Coordinate a, Coordinate b) => (byte)Math.Max(Math.Abs(a.x - b.x), Math.Abs(a.y - b.y));
}