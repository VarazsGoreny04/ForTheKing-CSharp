namespace ForTheKing;

public abstract class Tile(GameModel game, Coordinate position, byte hp, byte damage, byte range, byte speed)
{
	public class InitializeEventArgs : EventArgs { }

	public class PlaceEventArgs(Coordinate position, FieldNames field)
	{
		private readonly Coordinate position = position;
		private readonly FieldNames field = field;

		public Coordinate Position => position;
		public FieldNames Field => field;
	}

	public class MoveEventArgs(Coordinate oldPosition, Coordinate newPosition) : EventArgs
	{
		private readonly Coordinate oldPosition = oldPosition;
		private readonly Coordinate newPosition = newPosition;

		public Coordinate OldPosition => oldPosition;
		public Coordinate NewPosition => newPosition;
	}

	public class DieEventArgs(Coordinate position) : EventArgs
	{
		private readonly Coordinate position = position;

		public Coordinate Position => position;
	}

	protected GameModel game = game;
	protected Coordinate position = position;
	protected byte hp = hp;
	protected byte damage = damage;
	protected byte range = range;
	protected byte speed = speed;

	public Coordinate Position => position;
	public byte Hp => hp;
	public byte Damage => damage;
	public byte Range => range;

	public virtual void TakeHit(Tile enemy)
	{
		hp = (byte)(Math.Max(hp - enemy.damage, 0));

		if (hp == 0)
			game.OnDying(position);
	}

	public virtual Tile? Target() { return null; }

	public virtual void Attack() { }

	public virtual void Move() { }

	public virtual async Task Run(CancellationTokenSource token)
	{
		game.OnPlacingTile(position, Type());

		int delay = (int)(speed * GameModel.TICKTIME);

		while (!token.IsCancellationRequested && hp > 0)
		{
			await Task.Delay(delay);

			Tile? target = Target();

			if (target is Tile t && Coordinate.Distance(position, t.position) <= range)
				Attack();
			else
				Move();
		}
	}

	public abstract FieldNames Type();
}

public abstract class Ally(GameModel game, Coordinate position, byte hp, byte damage, byte range, byte speed) : Tile(game, position, hp, damage, range, speed)
{
	protected static Coordinate StdMove(Ally tile)
	{
		Tile? target = tile.Target();
		Coordinate targetPosition = target is Enemy a ? a.Position : new Coordinate(0, 0);
		Coordinate result = new(tile.position.X, tile.position.Y);

		if (Math.Abs(result.X - targetPosition.X) > Math.Abs(result.Y - targetPosition.Y))
			result.X = (sbyte)(Math.Sign(result.X) * (Math.Abs(result.X) - 1));
		else
			result.Y = (sbyte)(Math.Sign(result.Y) * (Math.Abs(result.Y) - 1));

		return result;
	}

	public abstract byte Cost();
}

public sealed class Castle(GameModel game, Coordinate position) : Ally(game, position, 100, 0, 0, 0)
{
	public override void TakeHit(Tile enemy)
	{
		hp = (byte)(Math.Max(hp - enemy.Damage, 0));

		if (hp == 0)
		{
			game.OnDying(position);
			game.End();
		}
	}

	public override Task Run(CancellationTokenSource token) { return Task.CompletedTask; }

	public override byte Cost() => throw new NotImplementedException();

	public override FieldNames Type() => FieldNames.Castle;
}

public sealed class Knight(GameModel game, Coordinate position) : Ally(game, position, 5, 1, 1, 2)
{
	public override Tile? Target()
	{
		return game.GetCircleArea(this).FindAll(x => x is Enemy).MinBy(x => Coordinate.Distance(position, x.Position));
	}

	public override void Attack()
	{
		List<Tile> enemiesInRange = game.GetBoxArea(this).FindAll(x => x is Enemy);

		foreach (Tile enemy in enemiesInRange)
			enemy.TakeHit(this);
	}

	public override byte Cost() => 5;

	public override FieldNames Type() => FieldNames.Ally;
}

public abstract class Enemy(GameModel game, Coordinate position, byte hp, byte damage, byte range, byte speed) : Tile(game, position, hp, damage, range, speed)
{
	public override Tile? Target()
	{
		return game.GetCircleArea(this).FindAll(x => x is Ally).MinBy(x => Coordinate.Distance(position, x.Position));
	}

	protected static Coordinate StdMove(Enemy tile)
	{
		Tile? target = tile.Target();
		Coordinate targetPosition = target is Ally a ? a.Position : new Coordinate(0, 0);
		Coordinate result = new(tile.position.X, tile.position.Y);

		if (Math.Abs(result.X - targetPosition.X) > Math.Abs(result.Y - targetPosition.Y))
			result.X = (sbyte)(Math.Sign(result.X) * (Math.Abs(result.X) - 1));
		else
			result.Y = (sbyte)(Math.Sign(result.Y) * (Math.Abs(result.Y) - 1));

		return result;
	}

	public override void Move()
	{
		Coordinate newPosition = StdMove(this);

		if (game[newPosition.X, newPosition.Y] is null)
		{
			game.OnMoving(position, newPosition);
			position = newPosition;
		}
	}
}

public sealed class Goblin(GameModel game, Coordinate position) : Enemy(game, position, 2, 1, 1, 2)
{
	public override void Attack() => game.GetPlusArea(this).FindAll(x => x is Ally).First().TakeHit(this);

	public override FieldNames Type() => FieldNames.Enemy;
}