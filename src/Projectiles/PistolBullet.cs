using Godot;
using System;

public partial class PistolBullet : Bullet
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

		CollisionAnimation.AnimationFinished += () => QueueFree();

		BulletAnimation.Visible = true;
		CollisionAnimation.Visible = false;
		CollisionAnimation.Stop();
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
