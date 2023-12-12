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
	{ get { return GetNode<AnimatedSprite2D>("PlayerSprite"); } }

	private AnimatedSprite2D MeleeAttackSprite
	{ get { return GetNode<AnimatedSprite2D>("MeleeAttackSprite"); } }

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

	private MeleeAttackHurtbox CreateMeleeHurtbox(CardinalDirection direction)
	{
		var hurtbox = MeleeHurtbox.Instantiate<MeleeAttackHurtbox>();
		hurtbox.GlobalRotationDegrees = RotationDegreesFromRight(direction);
		hurtbox.GlobalPosition = MeleeAttackSpawnMarker(direction).GlobalPosition;

		return hurtbox;
	}

	private Marker2D MeleeAttackSpawnMarker(CardinalDirection direction)
	{
		switch (direction)
		{
			case CardinalDirection.Left:
				return GetNode<Marker2D>("LeftMeleeAttackSpawn");

			case CardinalDirection.Right:
				return GetNode<Marker2D>("RightMeleeAttackSpawn");

			case CardinalDirection.Up:
				return GetNode<Marker2D>("UpMeleeAttackSpawn");

			default:
				return GetNode<Marker2D>("DownMeleeAttackSpawn");
		}
	}

	private float RotationDegreesFromRight(CardinalDirection direction)
	{
		switch (direction)
		{
			case CardinalDirection.Left:
				return 180;

			case CardinalDirection.Right:
				return 0;

			case CardinalDirection.Up:
				return 270;

			default:
				return 90;
		}
	}
	private void PlayMeleeAttackAnimation(CardinalDirection direction)
	{
		MeleeAttackSprite.Position = MeleeAttackSpawnMarker(direction).Position;
		MeleeAttackSprite.RotationDegrees = RotationDegreesFromRight(direction);

		MeleeAttackSprite.Visible = true;
		MeleeAttackSprite.Play("attack-right");
	}

	private void StopMeleeAttackAnimation()
	{
		MeleeAttackSprite.Visible = false;
		MeleeAttackSprite.Stop();
	}

	private void TriggerMeleeAttack(CardinalDirection direction)
	{
		if (_isMeleeAttacking)
		{
			return;
		}

		var hurtbox = CreateMeleeHurtbox(direction);

		_isMeleeAttacking = true;
		EmitSignal(SignalName.MeleeAttack, hurtbox);
		PlayMeleeAttackAnimation(direction);

		RunLater(0.2, () =>
		{
			_isMeleeAttacking = false;
			hurtbox.QueueFree();
			StopMeleeAttackAnimation();
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
