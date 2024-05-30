using System.Diagnostics;
using Godot;
using TreeTrunk;

public partial class PlayerCharacter : CharacterBody2D
{
	[Export]
	public float Speed { get; set; } = 200.0f;

	[Export]
	public float MaxHealth { get; set; } = 300.0f;

	private float _currentHealth;

	public PackedScene MeleeAttack = GD.Load<PackedScene>("res://src/Weapons/Melee/MeleeAttack.tscn");

	private bool _isMeleeAttacking = false;

	private RangedWeapon[] _weapons;

	private int _equippedWeaponIndex = 0;

	[Signal]
	public delegate void CurrentAmmoChangedEventHandler(int newCurrentAmmoValue);

	private void EmitCurrentAmmoChanged()
	{
		EmitSignal(SignalName.CurrentAmmoChanged, CurrentWeapon.CurrentAmmo);
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

	private RangedWeapon CurrentWeapon { get { return _weapons[_equippedWeaponIndex]; } }

	private bool IsDead
	{
		get { return _currentHealth <= 0; }
	}

	private bool IsMoving
	{
		get
		{
			return !IsDead && Velocity.Length() > 0;
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

	private void PlayMovementAnimation()
	{
		if (IsDead) return;

		if (IsMoving)
		{
			PlayRunningAnimation();
		}
		else
		{
			PlayIdleAnimation();
		}
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

	private MeleeAttack CreateMeleeAttack(CardinalDirection direction)
	{
		var attack = MeleeAttack.Instantiate<MeleeAttack>();
		attack.GlobalRotationDegrees = direction.RotationDegreesFromRight();
		attack.GlobalPosition = MeleeAttackSpawnMarker(direction).GlobalPosition;

		return attack;
	}

	private void PlayMeleeAttackAnimation(CardinalDirection direction)
	{
		MeleeAttackSprite.RotationDegrees = direction.RotationDegreesFromRight();
		MeleeAttackSprite.Position = MeleeAttackSpawnMarker(direction).Position;
		MeleeAttackSprite.Visible = true;
		MeleeAttackSprite.Play();
	}

	private void OnMeleeAttackAnimationFinished()
	{
		MeleeAttackSprite.Visible = false;
	}

	private void TriggerMeleeAttack(CardinalDirection direction)
	{
		if (_isMeleeAttacking || IsDead)
		{
			return;
		}

		var attack = CreateMeleeAttack(direction);

		_isMeleeAttacking = true;
		GetNode<GameManager>("/root/GameManager").AddToCurrentScene(attack);
		PlayMeleeAttackAnimation(direction);

		this.RunLater(0.2, () =>
		{
			_isMeleeAttacking = false;
			attack.QueueFree();
		}
		);
	}

	private void TriggerRangedAttack()
	{

		var bulletDirection = (GetGlobalMousePosition() - GlobalPosition).Normalized();
		var spawnPosition = RangedAttackSpawn.GlobalPosition + bulletDirection * 20;

		CurrentWeapon.TriggerRangedAttack(bulletDirection, spawnPosition);
	}

	public override void _Ready()
	{
		base._Ready();
		_currentHealth = MaxHealth;
		GetNode<Area2D>("HealthHitbox").AreaEntered += OnHealthHitboxAreaEntered;
		MeleeAttackSprite.AnimationFinished += OnMeleeAttackAnimationFinished;

		_weapons = new RangedWeapon[] { new Pistol(), new Shotgun() };

		foreach (var weapon in _weapons)
		{
			AddChild(weapon);
			weapon.Connect(RangedWeapon.SignalName.CurrentAmmoChanged,
				Callable.From<int>((_) => EmitCurrentAmmoChanged()));
		}
		EmitCurrentAmmoChanged();
	}

	public void CycleWeapon()
	{
		_equippedWeaponIndex = (_equippedWeaponIndex + 1) % _weapons.Length;
		EmitCurrentAmmoChanged();
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);

		if (IsDead) return;

		if (Input.IsActionJustPressed("melee_attack"))
		{
			TriggerMeleeAttack(DirectionOfPlayer());
		}
		else if (Input.IsActionJustPressed("cycle_weapon"))
		{
			CycleWeapon();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (IsDead) return;

		SetVelocityFromInput();
		PlayMovementAnimation();
		MoveAndSlide();

		if (Input.IsActionPressed("ranged_attack"))
		{
			TriggerRangedAttack();
		}
	}

	private void OnHealthHitboxAreaEntered(Area2D area)
	{
		var attack = area.GetParent<IAttack>();
		_currentHealth = Mathf.Clamp(_currentHealth - attack.Damage, 0, MaxHealth);

		if (IsDead)
		{
			Die();
		}
	}

	private void OnDeathAnimationFinished()
	{
		PlayerSprite.Visible = false;
		PlayerSprite.AnimationFinished -= OnDeathAnimationFinished;
	}

	private void Die()
	{
		GetNode<Sprite2D>("IdleShadow").Visible = false;
		PlayerSprite.Play("death");
		PlayerSprite.AnimationFinished += OnDeathAnimationFinished;

		GetNode<CollisionShape2D>("PhysicsHitbox").SetDeferred(
			CollisionShape2D.PropertyName.Disabled, true
			);
		GetNode<CollisionShape2D>("HealthHitbox/CollisionShape2D").SetDeferred(
			CollisionShape2D.PropertyName.Disabled, true
		);
		Debug.WriteLine("YOU DIED");
	}
}
