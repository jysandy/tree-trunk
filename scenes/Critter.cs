using System.Diagnostics;
using Godot;
using TreeTrunk;

public partial class Critter : CharacterBody2D
{
	[Export]
	public float Speed = 200.0f;

	[Export]
	public float MaxHealth = 200.0f;

	float _currentHealth = 0.0f;

	public bool IsDead { get { return _currentHealth <= 0; } }

	private NavigationAgent2D NavigationAgent
	{ get { return GetNode<NavigationAgent2D>("NavigationAgent2D"); } }

	private AnimatedSprite2D CritterSprite
	{ get { return GetNode<AnimatedSprite2D>("AnimatedSprite2D"); } }

	private Area2D HealthHitbox
	{ get { return GetNode<Area2D>("HealthHitbox"); } }

	private CollisionShape2D PhysicsHitbox
	{ get { return GetNode<CollisionShape2D>("PhysicsHitbox"); } }

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

		SetVelocityFromNavigation();
		MoveAndSlide();
		PlayAnimation();
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
