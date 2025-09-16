using ForTheKing;

namespace ForTheKingWFP.ViewModel;

public class Field : ViewModelBase
{
	private FieldNames _value;

	public sbyte X { get; set; }
	public sbyte Y { get; set; }

	public DelegateCommand? BuyCommand { get; set; }

	public FieldNames Value
	{
		get { return _value; }
		set
		{
			_value = value;
			OnPropertyChanged(nameof(Value));
		}
	}

	public Coordinate Position { get => new(X, Y); }

	public Field(sbyte X, sbyte Y, FieldNames Field = FieldNames.Empty)
	{
		this.X = X;
		this.Y = Y;
		this.Value = Field;
	}
}