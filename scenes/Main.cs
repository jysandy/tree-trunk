using Godot;

public partial class Main : Node2D
{
	public bool NavigationMapReady {get; set;} = false;

	public override void _Ready()
	{
		GD.Randomize();
		// Make sure to not await during _Ready.
		Callable.From(WaitForNavigationMap).CallDeferred();
	}

	private async void WaitForNavigationMap()
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
