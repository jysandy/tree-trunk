using Godot;
using System;
using System.Diagnostics;

public partial class PlayerCharacter : CharacterBody2D
{
	private enum CardinalDirection
	{
		Left, Right, Up, Down
	}

	[Export]
	public float Speed { get; set; } = 300.0f;

	private void SetVelocityFromInput()
	{
		Vector2 inputDirection = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		Velocity = inputDirection * Speed;
	}

	private AnimatedSprite2D PlayerSprite
	{
		get
		{
			return GetNode<AnimatedSprite2D>("PlayerSprite");
		}
	}

	private CardinalDirection ToCardinalDirection(Vector2 v)
	{
		if (v.X > 0 && (Mathf.Abs(v.X) > Mathf.Abs(v.Y)))
		{
			return CardinalDirection.Right;
		}
		else if (v.X < 0 && (Mathf.Abs(v.X) > Mathf.Abs(v.Y)))
		{
			return CardinalDirection.Left;
		}
		else if (v.Y < 0 && (Mathf.Abs(v.Y) > Mathf.Abs(v.X)))
		{
			return CardinalDirection.Up;
		}
		else
		{
			return CardinalDirection.Down;
		}
	}

	private bool IsMoving
	{
		get
		{
			return Velocity.Length() > 0;
		}
	}

	private void PlayIdleAnimation()
	{
		if (Velocity.Y > 0)
		{
			PlayerSprite.FlipH = false;
			PlayerSprite.Play("run-down");
		}
		else if (Velocity.Y < 0)
		{
			PlayerSprite.FlipH = false;
			PlayerSprite.Play("run-up");
		}
		else if (Velocity.X > 0)
		{
			PlayerSprite.FlipH = false;
			PlayerSprite.Play("run-right");
		}
		else if (Velocity.X < 0)
		{
			PlayerSprite.FlipH = true;
			PlayerSprite.Play("run-right");
		}

	}

	private void PlayRunningAnimation()
	{
		var playerToMouse = GetGlobalMousePosition() - Position;

		switch (ToCardinalDirection(playerToMouse))
		{
			case CardinalDirection.Left:
				PlayerSprite.FlipH = true;
				PlayerSprite.Play("idle-right");
				break;

			case CardinalDirection.Right:
				PlayerSprite.FlipH = false;
				PlayerSprite.Play("idle-right");
				break;

			case CardinalDirection.Up:
				PlayerSprite.FlipH = false;
				PlayerSprite.Play("idle-up");
				break;

			case CardinalDirection.Down:
				PlayerSprite.FlipH = false;
				PlayerSprite.Play("idle-down");
				break;
		}
	}

	private void PlayAnimation()
	{
		if (IsMoving)
		{
			PlayIdleAnimation();
		}
		else
		{
			PlayRunningAnimation();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		SetVelocityFromInput();
		PlayAnimation();
		MoveAndSlide();
	}
}
