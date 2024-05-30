using System.Linq;
using Godot;

public partial class Root : Node
{
	public Node CurrentScene { get; set; }
	public CanvasLayer HUD { get { return GetNode<CanvasLayer>("HUD"); } }

	public override void _Ready()
	{
		CurrentScene = GetChildren()
			.Where((node) => node.Name != "HUD")
			.First();
	}
}
