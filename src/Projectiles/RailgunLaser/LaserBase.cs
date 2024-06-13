using Godot;

public partial class LaserBase : Node2D
{
	public Color Color { get; set; }
	public float Radius {get; set;}
	public float AlphaMulMin { get; set; }
	public float FlickerFrequency { get; set; }

	public override void _Ready()
	{
		base._Ready();
		Visible = false;
	}

	public override void _Draw()
	{
		var shader = (ShaderMaterial)Material;
		shader.SetShaderParameter("color", Color);
		shader.SetShaderParameter("radius", Radius);
		shader.SetShaderParameter("alpha_mul_min", AlphaMulMin);
		shader.SetShaderParameter("flicker_frequency", FlickerFrequency);

		DrawCircle(Vector2.Zero, Radius, Color);
	}
}
