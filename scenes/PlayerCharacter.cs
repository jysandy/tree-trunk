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

	[Export]
	public PackedScene MeleeHurtbox;

	private bool _isMeleeAttacking = false;

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

	private CardinalDirection DirectionOfPlayer()
	{
		var playerToMouse = GetGlobalMousePosition() - Position;
		return ToCardinalDirection(playerToMouse);
	}

	private void PlayRunningAnimation()
	{
		switch (DirectionOfPlayer())
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

	[Signal]
	public delegate void MeleeAttackEventHandler(MeleeAttackHurtbox hurtbox);

	private void TriggerMeleeAttack(CardinalDirection direction)
	{
		if (_isMeleeAttacking)
		{
			return;
		}

		float rotation = 0;
		Vector2 position = Vector2.Zero;

		switch (direction)
		{
			case CardinalDirection.Left:
				rotation = Mathf.DegToRad(90);
				position = GetNode<Marker2D>("LeftMeleeAttackSpawn").GlobalPosition;
				break;

			case CardinalDirection.Right:
				rotation = Mathf.DegToRad(270);
				position = GetNode<Marker2D>("RightMeleeAttackSpawn").GlobalPosition;
				break;

			case CardinalDirection.Up:
				rotation = Mathf.DegToRad(180);
				position = GetNode<Marker2D>("UpMeleeAttackSpawn").GlobalPosition;
				break;

			case CardinalDirection.Down:
				rotation = Mathf.DegToRad(0);
				position = GetNode<Marker2D>("DownMeleeAttackSpawn").GlobalPosition;
				break;
		}

		var hurtbox = MeleeHurtbox.Instantiate<MeleeAttackHurtbox>();
		hurtbox.GlobalRotation = rotation;
		hurtbox.GlobalPosition = position;

		_isMeleeAttacking = true;
		EmitSignal(SignalName.MeleeAttack, hurtbox);

		RunLater(0.2, () =>
		{
			_isMeleeAttacking = false;
			hurtbox.QueueFree();
		}
		);
	}

	private async void RunLater(double timeSeconds, Action f)
	{
		await ToSignal(GetTree().CreateTimer(timeSeconds), SceneTreeTimer.SignalName.Timeout);
		f();
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);

		if (Input.IsActionJustPressed("melee_attack"))
		{
			TriggerMeleeAttack(DirectionOfPlayer());
		}
	}
}
