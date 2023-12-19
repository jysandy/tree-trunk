using Godot;
using System;
using TreeTrunk;

public partial class Bullet : CharacterBody2D
{
	[Export]
	public float Damage { get; set; } = 30;

	public override void _Ready()
	{
		GetNode<IAttack>("BulletHurtbox").Damage = Damage;
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		var collision = MoveAndCollide(Velocity * (float)delta);

		if (collision != null)
		{
			QueueFree();
		}
	}
	private void OnBulletHurtboxAreaEntered(Area2D area)
	{
		QueueFree();
	}
}
