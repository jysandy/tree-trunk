using Godot;

public partial class LaserBase : Node2D
{
	public Color Color { get; set; }
	public float Radius { get; set; }
	public float AlphaMulMin { get; set; }
	public float FlickerFrequency { get; set; }
	public float FadeoutRate { get; set; }

	bool _fadingOut = false;

	[Signal]
	public delegate void FadeoutFinishedEventHandler();

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

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (_fadingOut)
		{
			Radius -= FadeoutRate * (float)delta;

			if (Radius <= 0)
			{
				Radius = 0;
				EmitSignal(SignalName.FadeoutFinished);
			}
			else
			{
				QueueRedraw();
			}
		}
	}

	public void BeginFadeout()
	{
		_fadingOut = true;
	}
}
