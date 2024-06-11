using Godot;
using System.Threading.Tasks;

public partial class Main : Node2D, TreeTrunk.ILevelRoot
{
	public bool NavigationMapReady {get; set;} = false;

	public override void _Ready()
	{
		GD.Randomize();
		// Make sure to not await during _Ready.
		Callable.From(WaitForNavigationMap).CallDeferred();
	}

	private async Task WaitForNavigationMap()
	{
		// Wait for the first physics frame so the NavigationServer can sync.
		await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
		NavigationMapReady = true;
	}

	public PlayerCharacter Player
	{
		get { return GetNode<PlayerCharacter>("PlayerCharacter"); }
	}
}
