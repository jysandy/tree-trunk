using Godot;
using TreeTrunk;

public partial class PlayerCharacter : CharacterBody2D
{
	[Export]
	public float Speed { get; set; } = 300.0f;

	[Export]
	public PackedScene MeleeHurtbox;

	private bool _isMeleeAttacking = false;

	[Signal]
	public delegate void SpawnInMainEventHandler(Node2D node);

	[Signal]
	public delegate void CurrentAmmoChangedEventHandler(int newCurrentAmmoValue);


	private void OnPistolCurrentAmmoChanged(long newCurrentAmmoValue)
	{
		EmitSignal(SignalName.CurrentAmmoChanged, newCurrentAmmoValue);
	}

	private void SetVelocityFromInput()
	{
		Vector2 inputDirection = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		Velocity = inputDirection * Speed;
	}

	private AnimatedSprite2D PlayerSprite
	{ get { return GetNode<AnimatedSprite2D>("PlayerSprite"); } }

	private AnimatedSprite2D MeleeAttackSprite
	{ get { return GetNode<AnimatedSprite2D>("MeleeAttackSprite"); } }

	private Marker2D RangedAttackSpawn { get { return GetNode<Marker2D>("RangedAttackSpawn"); } }

	private bool IsMoving
	{
		get
		{
			return Velocity.Length() > 0;
		}
	}

	private void PlayRunningAnimation()
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
		return CardinalDirectionExtension.FromVector(playerToMouse);
	}

	private void PlayIdleAnimation()
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
			PlayRunningAnimation();
		}
		else
		{
			PlayIdleAnimation();
		}
	}

	private MeleeAttackHurtbox CreateMeleeHurtbox(CardinalDirection direction)
	{
		var hurtbox = MeleeHurtbox.Instantiate<MeleeAttackHurtbox>();
		hurtbox.GlobalRotationDegrees = direction.RotationDegreesFromRight();
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

	private void PlayMeleeAttackAnimation(CardinalDirection direction)
	{
		MeleeAttackSprite.Position = MeleeAttackSpawnMarker(direction).Position;
		MeleeAttackSprite.RotationDegrees = direction.RotationDegreesFromRight();

		MeleeAttackSprite.Visible = true;
		MeleeAttackSprite.Play("attack-right");
	}

	private void StopMeleeAttackAnimation()
	{
		MeleeAttackSprite.Visible = false;
		MeleeAttackSprite.Stop();
	}

	private void AddChildToMain(Node2D node)
	{
		EmitSignal(SignalName.SpawnInMain, node);
	}

	private void TriggerMeleeAttack(CardinalDirection direction)
	{
		if (_isMeleeAttacking)
		{
			return;
		}

		var hurtbox = CreateMeleeHurtbox(direction);

		_isMeleeAttacking = true;
		AddChildToMain(hurtbox);
		PlayMeleeAttackAnimation(direction);

		this.RunLater(0.2, () =>
		{
			_isMeleeAttacking = false;
			hurtbox.QueueFree();
			StopMeleeAttackAnimation();
		}
		);
	}

	private void TriggerRangedAttack()
	{

		var bulletDirection = (GetGlobalMousePosition() - GlobalPosition).Normalized();

		var bullet = GetNode<Pistol>("Pistol")
		.TriggerRangedAttack(bulletDirection);

		if (bullet != null)
		{
			bullet.GlobalPosition = RangedAttackSpawn.GlobalPosition + bulletDirection * 20;
			AddChildToMain(bullet);
		}
	}

	public override void _Ready()
	{
		base._Ready();
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);

		if (Input.IsActionJustPressed("melee_attack"))
		{
			TriggerMeleeAttack(DirectionOfPlayer());
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		SetVelocityFromInput();
		PlayAnimation();
		MoveAndSlide();

		if (Input.IsActionPressed("ranged_attack"))
		{
			TriggerRangedAttack();
		}
	}
}
