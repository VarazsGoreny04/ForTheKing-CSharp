namespace ForTheKing;

public abstract class Tile
{
	private Coordinate position;
	private byte hp;
	public byte damage;
	public byte range;

	public Coordinate Position => position;
	public byte Hp => hp;
	public byte Damage => damage;

	protected Tile(Coordinate position, byte hp, byte damage, byte range)
	{
		this.position = position;
		this.hp = hp;
		this.damage = damage;
		this.range = range;
	}

	protected bool TakeHit(Tile enemy)
	{
		bool dies = hp < enemy.damage;

		hp = (byte)(dies ? hp - enemy.damage : 0);

		return dies;
	}

	public abstract void Attack();

	public abstract byte Cost();
}

public abstract class Ally : Tile
{
	protected Ally(Coordinate position, byte hp, byte damage, byte range) : base(position, hp, damage, range) { }
}

public class Castle : Tile
{
	public Castle(Coordinate position) : base(position, 100, 0, 0) { }

	public override void Attack() { }

	public override byte Cost() => throw new NotImplementedException();
}

public class Knight : Tile
{
	public Knight(Coordinate position) : base(position, 5, 1, 1) { }

	public override void Attack()
	{

	}

	public override byte Cost() => 1;
}

public abstract class Enemy : Tile
{
	protected Enemy(Coordinate position, byte hp, byte damage, byte range) : base(position, hp, damage, range) { }
}