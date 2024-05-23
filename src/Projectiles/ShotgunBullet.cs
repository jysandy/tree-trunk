using Godot;
using System;
using TreeTrunk;

public partial class ShotgunBullet : Bullet
{
	private AnimatedSprite2D BulletAnimation
	{
		get { return GetNode<AnimatedSprite2D>("BulletAnimation"); }
	}

	private AnimatedSprite2D CollisionAnimation
	{
		get { return GetNode<AnimatedSprite2D>("CollisionAnimation"); }
	}

	public override void _Ready()
	{
		base._Ready();
		// TODO: Remove shotgun bullets after they travel a certain distance,
		// rather than after a certain amount of time
		this.RunLater(0.2f, () => QueueFree());

		CollisionAnimation.AnimationFinished += () => QueueFree();

		BulletAnimation.Visible = true;
		CollisionAnimation.Visible = false;
	}

	protected override void OnBulletCollided()
	{
		if (CollisionAnimation.IsPlaying())
		{
			return;
		}

		Velocity = Vector2.Zero;
		BulletAnimation.Stop();
		BulletAnimation.Visible = false;
		CollisionAnimation.Visible = true;
		CollisionAnimation.Play();
	}
}
