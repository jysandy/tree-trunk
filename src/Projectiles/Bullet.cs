using Godot;
using TreeTrunk;

// Attach this script to any Bullet.
public partial class Bullet : CharacterBody2D, IAttack
{
	[Export]
	public float Damage { get; set; } = 30;

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
		bulletHurtbox.CollisionLayer = 0;
		bulletHurtbox.CollisionMask = 0;
		bulletHurtbox.SetCollisionLayerValue(4, true); // player_attack_hurtbox
		bulletHurtbox.SetCollisionMaskValue(2, true); // enemy_hitboxes
		bulletHurtbox.AreaEntered += OnBulletHurtboxAreaEntered;

		AddChild(bulletHurtbox);

		// So that we don't have to set these in the editor
		MotionMode = MotionModeEnum.Floating;
		CollisionLayer = 0;
		CollisionMask = 0;
		SetCollisionMaskValue(1, true);
		ZIndex = 1;
		YSortEnabled = true;
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		var collision = MoveAndCollide(Velocity * (float)delta);

		if (collision != null)
		{
			OnBulletCollided();
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
