using System.Linq;
using Godot;

public partial class Root : Node
{
	public Node CurrentScene { get; set; }
	public HUD HUD { get { return GetNode<HUD>("HUD"); } }

	public override void _Ready()
	{
		CurrentScene = GetChildren()
			.Where((node) => node.Name != "HUD")
			.First();
	}
}
