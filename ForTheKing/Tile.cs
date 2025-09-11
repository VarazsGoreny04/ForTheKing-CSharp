namespace ForTheKing;

public abstract class Tile(Coordinate position, byte hp, byte damage, byte range)
{
	protected Coordinate position = position;
	protected byte hp = hp;
	protected byte damage = damage;
	protected byte range = range;

	public Coordinate Position => position;
	public byte Hp => hp;
	public byte Damage => damage;
	public byte Range => range;

	public bool TakeHit(Tile enemy)
	{
		bool dies = hp < enemy.damage;

		hp = (byte)(dies ? hp - enemy.damage : 0);

		return dies;
	}

	public virtual void Attack() { }

	public virtual void Move() { }
}

public abstract class Ally(Coordinate position, byte hp, byte damage, byte range) : Tile(position, hp, damage, range)
{
	public abstract byte Cost();

	protected static void StdMove(Ally tile)
	{
		Tile? target = Game.GetCircleArea(tile).FindAll(x => x is Enemy).MinBy(x => Coordinate.Distance(tile.position, x.Position));
		Coordinate targetPosition = target is Enemy a ? a.Position : new Coordinate(0, 0);
		Coordinate c = tile.Position;

		if (Math.Abs(c.X - targetPosition.X) > Math.Abs(c.Y - targetPosition.Y))
			c.X = (sbyte)(Math.Sign(c.X) * (Math.Abs(c.X) - 1));
		else
			c.Y = (sbyte)(Math.Sign(c.Y) * (Math.Abs(c.Y) - 1));
	}
}

public class Castle(Coordinate position) : Ally(position, 100, 0, 0)
{
	public override byte Cost() => throw new NotImplementedException();
}

public class Knight(Coordinate position) : Ally(position, 5, 1, 1)
{
	public override void Attack() => Game.GetBoxArea(this).FindAll(x => x is Enemy).ForEach(x => x.TakeHit(this));

	public override byte Cost() => 1;
}

public abstract class Enemy(Coordinate position, byte hp, byte damage, byte range) : Tile(position, hp, damage, range)
{
	protected static void StdMove(Enemy tile)
	{
		Tile? target = Game.GetCircleArea(tile).FindAll(x => x is Ally).MinBy(x => Coordinate.Distance(tile.position, x.Position));
		Coordinate targetPosition = target is Ally a ? a.Position : new Coordinate(0, 0);
		Coordinate c = tile.Position;

		if (Math.Abs(c.X - targetPosition.X) > Math.Abs(c.Y - targetPosition.Y))
			c.X = (sbyte)(Math.Sign(c.X) * (Math.Abs(c.X) - 1));
		else
			c.Y = (sbyte)(Math.Sign(c.Y) * (Math.Abs(c.Y) - 1));
	}
}

public class Goblin(Coordinate position) : Enemy(position, 2, 1, 1)
{
	public override void Attack() => Game.GetBoxArea(this).FindAll(x => x is Ally)?.First().TakeHit(this);

	public override void Move()
	{
		StdMove(this);
	}
}