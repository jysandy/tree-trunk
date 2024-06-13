using Godot;
using TreeTrunk;

public partial class RailgunLaser : Node2D, IAttack
{
	[Export]
	public float MaxLength { get; set; } = 300.0f;

	[Export]
	public float Damage { get; set; } = 150.0f;

	private bool _lengthSet = false;

	private float _length = 0.0f;

	private GameManager GameManager { get { return GetNode<GameManager>("/root/GameManager"); } }

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		if (_lengthSet) return;


		// TODO: make this a shape collision test?
		uint collisionMask = GameManager.BuildPhysicsLayerMask("wall_collisions");
		var raycastEnd = GlobalPosition + new Vector2(1, 0).Rotated(GlobalRotation).Normalized() * MaxLength;
		var query = PhysicsRayQueryParameters2D.Create(
			GlobalPosition,
			raycastEnd,
			collisionMask
		);
		query.CollideWithAreas = true;

		var raycastResult = GetWorld2D().DirectSpaceState.IntersectRay(query);

		if (raycastResult.Count == 0)
		{
			SetLength(MaxLength);
			return;
		}
		else
		{
			var collisionPositionProjection = ((Vector2)raycastResult["position"] - GlobalPosition)
				.Project(raycastEnd - GlobalPosition);
			SetLength(collisionPositionProjection.Length() + 5);
			return;
		}
	}

	private void SetLength(float length)
	{
		var newShape = new RectangleShape2D
		{
			Size = new Vector2(length, 5)
		};

		GetNode<CollisionShape2D>("LaserHurtbox/CollisionShape2D").Shape = newShape;
		GetNode<Area2D>("LaserHurtbox").Position = new Vector2((float)length / 2, 0);
		GetNode<CollisionShape2D>("LaserHurtbox/CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
		_length = length;
		_lengthSet = true;
		QueueRedraw();
	}

	public override void _Draw()
	{
		base._Draw();
		if (_lengthSet == false) return;
		DrawLine(new Vector2(0, 0), 
				 new Vector2(1, 0) * _length,
				 Colors.Green,
				 5.0f);
	}
}
