using Godot;
using System;
using System.Diagnostics;

public partial class Main : Node2D
{
	private bool _navigationMapReady = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Make sure to not await during _Ready.
		Callable.From(WaitForNavigationMap).CallDeferred();
	}

	private async void WaitForNavigationMap()
	{
		// Wait for the first physics frame so the NavigationServer can sync.
		await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
		_navigationMapReady = true;
	}

	private PlayerCharacter Player
	{
		get { return GetNode<PlayerCharacter>("PlayerCharacter"); }
	}

	private Critter Critter
	{
		get { return GetNode<Critter>("Critter"); }
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_navigationMapReady)
		{
			Critter.MovementTarget = Player.GlobalTransform.Origin;
		}
	}

	private void OnAddChildToMain(Node2D node)
	{
		AddChild(node);
	}
}
