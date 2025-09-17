using ForTheKing.Model;

namespace ForTheKingWFP.ViewModel;

public class FieldEventArgs(Field field, Coordinate oldPosition, Coordinate newPosition) : EventArgs
{
	private readonly Field field = field;
	private readonly Coordinate oldPosition = oldPosition;
	private readonly Coordinate newPosition = newPosition;

	public Field Field => field;
	public Coordinate OldPosition => oldPosition;
	public Coordinate NewPosition => newPosition;
}