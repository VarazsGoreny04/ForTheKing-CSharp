using ForTheKing;
using ForTheKingBlazor.Components.ViewModel;

namespace ForTheKingBlazor.Components.View;

public partial class Home
{
	private readonly GameModel _model = null!;
	private readonly GameViewModel _viewModel = null!;

	private uint Gold => _model.Gold;
	private uint Timer => _model.Timer;

	public Home()
	{
		_model = new GameModel();
		_viewModel = new GameViewModel(_model);

		_model.TimerTicking += (_, _) => InvokeAsync(StateHasChanged);
		_model.GoldChanging += (_, _) => InvokeAsync(StateHasChanged);

		NewGame();
	}

	private void NewGame()
	{
		_model.OnCreatingGame();
	}

	private void Buy(int x, int y)
	{
		_model.Buy(new Knight(_model, new Coordinate((sbyte)(x - GameModel.MAPRADIUS), (sbyte)(y - GameModel.MAPRADIUS))));
	}
}