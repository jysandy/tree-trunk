using Godot;
using System;
using TreeTrunk;

public partial class ShotgunBullet : Bullet
{
	public override void _Ready()
	{
		base._Ready();
		// TODO: Remove shotgun bullets after they travel a certain distance,
		// rather than after a certain amount of time
		this.RunLater(0.2f, () => QueueFree());
	}
}
