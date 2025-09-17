using ForTheKing.Model;
using ForTheKingBlazor.Components.ViewModel;
using Microsoft.AspNetCore.Components;

namespace ForTheKingBlazor.Components.View;

public partial class Cell
{
	[Parameter] public int X { get; set; }
	[Parameter] public int Y { get; set; }
	[Parameter] public Field Field { get; set; } = null!;
	[Parameter] public Action<int, int>? OnClick { get; set; }

	private string Color => Field.Value switch
	{
		FieldNames.Empty => "lime",
		FieldNames.Castle => "grey",
		FieldNames.Ally => "green",
		FieldNames.Enemy => "red",
		_ => "black"
	};

	protected override void OnParametersSet()
	{
		Field.PropertyChanged += (_, _) => InvokeAsync(StateHasChanged);
	}
}