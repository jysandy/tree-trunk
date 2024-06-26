using Godot;
using TreeTrunk;

// Attach this script to any Bullet.
public partial class Bullet : CharacterBody2D, IAttack
{
	public GameManager GameManager { get { return GetNode<GameManager>("/root/GameManager"); } }

	public enum BulletTypeEnum
	{
		PlayerFired,
		EnemyFired
	}

	[Export]
	public float Damage { get; set; } = 30;

	// Who fired the bullet.
	// Affects the entities that the bullet detects collisions with.
	[Export]
	public virtual BulletTypeEnum BulletType { get; set; } = BulletTypeEnum.PlayerFired;

	[Export]
	public virtual float MaxRange { get; set; } = 130;

	private Vector2 _startingGlobalPosition;

	// Returns the Shape2D assigned to the CollisionPolygon2D or CollisionShape2D
	// child of this node.
	private Shape2D GetShape2DFromChildren()
	{
		var shapeOwners = GetShapeOwners();
		if (shapeOwners.IsEmpty())
		{
			return null;
		}

		return ShapeOwnerGetShape((uint)shapeOwners[0], 0);
	}

	public override void _Ready()
	{
		var bulletHurtbox = new Area2D();
		var hurtboxCollisionShape = new CollisionShape2D
		{
			Shape = GetShape2DFromChildren()
		};
		bulletHurtbox.AddChild(hurtboxCollisionShape);

		if (BulletType == BulletTypeEnum.PlayerFired)
		{
			bulletHurtbox.CollisionLayer = GameManager.BuildPhysicsLayerMask("player_attack_hurtbox");
			bulletHurtbox.CollisionMask = GameManager.BuildPhysicsLayerMask("enemy_hitboxes");
		}
		else if (BulletType == BulletTypeEnum.EnemyFired)
		{
			bulletHurtbox.CollisionLayer = GameManager.BuildPhysicsLayerMask("enemy_attack_hurtbox");
			bulletHurtbox.CollisionMask = GameManager.BuildPhysicsLayerMask("player_hitbox");
		}

		bulletHurtbox.AreaEntered += OnBulletHurtboxAreaEntered;

		AddChild(bulletHurtbox);

		// So that we don't have to set these in the editor
		MotionMode = MotionModeEnum.Floating;
		CollisionLayer = 0;
		CollisionMask = GameManager.BuildPhysicsLayerMask("wall_collisions");
		ZIndex = 1;
		YSortEnabled = true;

		_startingGlobalPosition = GlobalPosition;
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		var collision = MoveAndCollide(Velocity * (float)delta);

		if (collision != null)
		{
			OnBulletCollided();
		}

		if ((GlobalPosition - _startingGlobalPosition).Length() >= MaxRange)
		{
			QueueFree();
		}
	}
	private void OnBulletHurtboxAreaEntered(Area2D area)
	{
		OnBulletCollided();
	}

	protected virtual void OnBulletCollided()
	{
		QueueFree();
	}
}
