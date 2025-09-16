using ForTheKing;
using ForTheKingBlazor.Components.ViewModel;

namespace ForTheKingBlazor.Components.View;

public partial class Home
{
	private GameModel _model = null!;
	private GameViewModel _viewModel = null!;

	private uint Gold => _model.Gold;
	private uint Timer => _model.Timer;

	public Home()
	{
		_model = new GameModel();
		_viewModel = new GameViewModel(_model);

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