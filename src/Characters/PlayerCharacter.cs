using System.Diagnostics;
using Godot;
using TreeTrunk;

public partial class PlayerCharacter : CharacterBody2D
{
	[Export]
	public float Speed { get; set; } = 200.0f;

	[Export]
	public float MaxHealth { get; set; } = 10000.0f;

	private float _currentHealth;
	private RangedWeapon[] _weapons;
	private int _equippedWeaponIndex = 0;

	[Signal]
	public delegate void CurrentAmmoChangedEventHandler(int newCurrentAmmoValue);

	public RangedWeapon CurrentWeapon { get { return _weapons[_equippedWeaponIndex]; } }

	public int CurrentAmmo { get { return CurrentWeapon.CurrentAmmo; } }

	public bool IsDead
	{
		get { return _currentHealth <= 0; }
	}

	private void EmitCurrentAmmoChanged()
	{
		EmitSignal(SignalName.CurrentAmmoChanged, CurrentWeapon.CurrentAmmo);
	}

	public void CycleWeapon()
	{
		CurrentWeapon.Unequip();
		_equippedWeaponIndex = (_equippedWeaponIndex + 1) % _weapons.Length;
		CurrentWeapon.Equip();
		EmitCurrentAmmoChanged();
	}

	public void TakeDamage(float damage)
	{
		_currentHealth = Mathf.Clamp(_currentHealth - damage, 0, MaxHealth);
	}

	// Weapons have to be scenes in order to contain AudioStreamPlayers with WAVs.
	private PackedScene _pistolScene = GD.Load<PackedScene>("res://src/Weapons/Ranged/Pistol.tscn");
	private PackedScene _railgunScene = GD.Load<PackedScene>("res://src/Weapons/Ranged/Railgun.tscn");
	private PackedScene _shotgunScene = GD.Load<PackedScene>("res://src/Weapons/Ranged/Shotgun.tscn");

	public PackedScene MeleeAttack = GD.Load<PackedScene>("res://src/Weapons/Melee/MeleeAttack.tscn");

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

	public Area2D HealthHitbox
	{ get { return GetNode<Area2D>("HealthHitbox"); } }

	public Marker2D CentreOfMass
	{ get { return GetNode<Marker2D>("CentreOfMass"); } }

	private Marker2D RangedAttackSpawn { get { return GetNode<Marker2D>("RangedAttackSpawn"); } }

	public GameManager GameManager { get { return GetNode<GameManager>("/root/GameManager"); } }

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
		GameManager.AddToCurrentScene(attack);
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
		CurrentWeapon.TriggerRangedAttack();
	}

	public override void _Ready()
	{
		base._Ready();
		GetNode<Area2D>("HealthHitbox").AreaEntered += OnHealthHitboxAreaEntered;
		MeleeAttackSprite.AnimationFinished += OnMeleeAttackAnimationFinished;

		_currentHealth = MaxHealth;
		_weapons = new RangedWeapon[] {
				_pistolScene.Instantiate<Pistol>(),
				_shotgunScene.Instantiate<Shotgun>(),
				_railgunScene.Instantiate<Railgun>()
			 };
		foreach (var weapon in _weapons)
		{
			weapon.Unequip();
			AddChild(weapon);
			weapon.Position = RangedAttackSpawn.Position;
			weapon.Connect(RangedWeapon.SignalName.CurrentAmmoChanged,
				Callable.From<int>((_) => EmitCurrentAmmoChanged()));
		}
		CurrentWeapon.Equip();
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
		TakeDamage(attack.Damage);

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
