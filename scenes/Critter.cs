using Godot;
using TreeTrunk;

public partial class Critter : CharacterBody2D
{
	[Export]
	public float Speed = 200.0f;

	[Export]
	public float MaxHealth = 200.0f;

	private float _currentHealth = 0.0f;

	private bool _canShoot = true;

	[Signal]
	public delegate void SpawnInMainEventHandler(Node2D node);

	public bool NavigationMapReady { get { return GetParent<Main>().NavigationMapReady; } }

	public bool IsDead { get { return _currentHealth <= 0; } }

	public PackedScene BulletScene = GD.Load<PackedScene>("res://src/Projectiles/CritterBullet.tscn");

	private PlayerCharacter Player
	{
		get { return GetParent<Main>().Player; }
	}

	private NavigationAgent2D NavigationAgent
	{ get { return GetNode<NavigationAgent2D>("NavigationAgent2D"); } }

	private AnimatedSprite2D CritterSprite
	{ get { return GetNode<AnimatedSprite2D>("AnimatedSprite2D"); } }

	private Area2D HealthHitbox
	{ get { return GetNode<Area2D>("HealthHitbox"); } }

	private CollisionShape2D PhysicsHitbox
	{ get { return GetNode<CollisionShape2D>("PhysicsHitbox"); } }

	private Marker2D RangedAttackSpawn
	{ get { return GetNode<Marker2D>("RangedAttackSpawn"); } }

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

	private void AddChildToMain(Node2D node)
	{
		EmitSignal(SignalName.SpawnInMain, node);
	}

	public override void _Ready()
	{
		base._Ready();

		_currentHealth = MaxHealth;
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
		if (IsDead)
		{
			return;
		}

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

	private void FireBullet()
	{
		var bulletDirection = (Player.GlobalPosition - GlobalPosition).Normalized();
		var bullet = BulletScene.Instantiate<CritterBullet>();
		bullet.GlobalPosition = RangedAttackSpawn.GlobalPosition + bulletDirection * 20;
		bullet.Velocity = bulletDirection * 400.0f;
		bullet.GlobalRotation = bulletDirection.Angle();
		AddChildToMain(bullet);
	}

	private void SetVelocityFromNavigation()
	{
		if (!NavigationAgent.IsTargetReachable() || NavigationAgent.IsNavigationFinished())
		{
			Velocity = Vector2.Zero;
			return;
		}

		Vector2 currentAgentPosition = GlobalPosition;
		Vector2 nextPathPosition = NavigationAgent.GetNextPathPosition();

		Velocity = currentAgentPosition.DirectionTo(nextPathPosition) * Speed;
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (IsDead)
		{
			return;
		}

		if (NavigationMapReady)
		{
			MovementTarget = Player.GlobalTransform.Origin;
			SetVelocityFromNavigation();
			MoveAndSlide();
		}

		PlayAnimation();

		if (_canShoot)
		{
			FireBullet();
			_canShoot = false;
			this.RunLater(2.0, () => _canShoot = true);
		}
	}

	private void Die()
	{
		CritterSprite.Play("dead-right");
		GetNode<CollisionShape2D>("PhysicsHitbox").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
		GetNode<CollisionShape2D>("HealthHitbox/CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
	}

	private void OnCritterHitboxAreaEntered(Area2D area)
	{
		if (IsDead)
		{
			return;
		}

		var attack = area.GetParent<IAttack>();

		_currentHealth = Mathf.Clamp(_currentHealth - attack.Damage, 0, MaxHealth);

		if (IsDead)
		{
			Die();
		}
	}
}
