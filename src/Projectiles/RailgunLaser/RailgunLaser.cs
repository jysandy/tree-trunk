using Godot;
using TreeTrunk;

public partial class RailgunLaser : Node2D, IAttack
{
	[Export]
	public float MaxLength { get; set; } = 300.0f;

	[Export]
	public float BeamVisualWidth { get; set; } = 10.0f;

	[Export]
	public float HurtboxWidth { get; set; } = 5.0f;

	[Export]
	public Color BeamColor { get; set; } = Colors.Green;

	[Export]
	public float AlphaMulMin { get; set; } = 0.5f;

	[Export]
	public float FlickerFrequency { get; set; } = 30;

	[Export]
	public float Damage { get; set; } = 150.0f;

	private bool _lengthSet = false;

	private float _length = 0.0f;

	private bool _fadingOut = false;

	[Export]
	public float FadeoutRate { get; set; } = 33.3333f;

	private GameManager GameManager { get { return GetNode<GameManager>("/root/GameManager"); } }
	private LaserBase LaserBase { get { return GetNode<LaserBase>("LaserBase"); } }
	private CollisionShape2D CollisionShape { get { return GetNode<CollisionShape2D>("LaserHurtbox/CollisionShape2D"); } }

	public override void _Ready()
	{
		base._Ready();
		LaserBase.Color = BeamColor;
		LaserBase.Radius = (BeamVisualWidth / 2.0f) * 2.5f;
		LaserBase.AlphaMulMin = AlphaMulMin;
		LaserBase.FlickerFrequency = FlickerFrequency;
		LaserBase.FadeoutRate = FadeoutRate;
		LaserBase.FadeoutFinished += OnLaserBaseFadeoutFinished;
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		if (_lengthSet) return;


		// TODO: make this a shape collision test?
		uint collisionMask = GameManager.BuildPhysicsLayerMask("wall_collisions");
		var raycastEnd = GlobalPosition + new Vector2(1, 0).Rotated(GlobalRotation) * MaxLength;
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
			Size = new Vector2(length, HurtboxWidth)
		};

		CollisionShape.Shape = newShape;
		GetNode<Area2D>("LaserHurtbox").Position = new Vector2((float)length / 2, 0);
		CollisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
		_length = length;
		_lengthSet = true;
		LaserBase.Visible = true;
		QueueRedraw();
	}

	public override void _Draw()
	{
		base._Draw();
		if (_lengthSet == false) return;

		var shader = (ShaderMaterial)Material;
		shader.SetShaderParameter("beam_width", BeamVisualWidth);
		shader.SetShaderParameter("beam_color", BeamColor);
		shader.SetShaderParameter("alpha_mul_min", AlphaMulMin);
		shader.SetShaderParameter("flicker_frequency", FlickerFrequency);

		DrawRect(new Rect2(new Vector2(0, -(BeamVisualWidth / 2)),
						   new Vector2(_length, BeamVisualWidth)),
				 Colors.Green);
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (_fadingOut)
		{
			BeamVisualWidth -= FadeoutRate * (float)delta;
			if (BeamVisualWidth <= 0)
			{
				BeamVisualWidth = 0;
			}
			else
			{
				QueueRedraw();
			}
		}
	}

	public void RemoveHitboxAndFadeout()
	{
		CollisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
		this.RunLater(0.1, BeginFadeout);
	}

	private void BeginFadeout()
	{
		_fadingOut = true;
		LaserBase.BeginFadeout();
	}

	public void OnLaserBaseFadeoutFinished()
	{
		// The base should fade out after the ray,
		// since the base is wider.
		QueueFree();
	}
}
