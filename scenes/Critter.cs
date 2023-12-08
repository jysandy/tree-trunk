using System.Diagnostics;
using Godot;

public partial class Critter : CharacterBody2D
{
	[Export]
	public float Speed = 200.0f;

	private NavigationAgent2D NavigationAgent
	{
		get { return GetNode<NavigationAgent2D>("NavigationAgent2D"); }
	}

	private AnimatedSprite2D CritterSprite
	{
		get { return GetNode<AnimatedSprite2D>("AnimatedSprite2D"); }
	}

	public Vector2 MovementTarget
	{
		get { return NavigationAgent.TargetPosition; }
		set
		{
			if (value != NavigationAgent.TargetPosition)
			{
				NavigationAgent.TargetPosition = value;
			}
		}
	}

	public override void _Ready()
	{
		base._Ready();

		PlayAnimation();
	}

	private bool IsMoving
	{
		get 
		{
			return Velocity.Length() > 0;
		}
	}

	private void PlayAnimation()
	{
		if (IsMoving)
		{
			if (Velocity.X > 0)
			{
				CritterSprite.FlipH = false;
				CritterSprite.Play("run-right");
			}
			else if (Velocity.X < 0)
			{
				CritterSprite.FlipH = true;
				CritterSprite.Play("run-right");
			}
		}
		else 
		{
			CritterSprite.Play("idle");
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (!NavigationAgent.IsTargetReachable())
		{
			Debug.WriteLine("Target not reachable!");
			PlayAnimation();
			return;
		}

		if (NavigationAgent.IsNavigationFinished())
		{
			Velocity = Vector2.Zero;
			PlayAnimation();
			return;
		}

		Vector2 currentAgentPosition = GlobalPosition; // try replacing with GlobalPosition
		Vector2 nextPathPosition = NavigationAgent.GetNextPathPosition();

		Velocity = currentAgentPosition.DirectionTo(nextPathPosition) * Speed;
		MoveAndSlide();
		PlayAnimation();
	}
}
