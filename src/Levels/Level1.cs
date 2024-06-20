using Godot;
using System.Threading.Tasks;

namespace TreeTrunk
{
	public partial class Level1 : Node2D, ILevelRoot
	{
		public bool NavigationMapReady { get; set; } = false;

		private GameManager GameManager
		{ get { return GetNode<GameManager>("/root/GameManager"); } }

		public override void _Ready()
		{
			GD.Randomize();
			// Make sure to not await during _Ready.
			Callable.From(WaitForNavigationMap).CallDeferred();
			GameManager.InitializeHUD();
		}

		private async Task WaitForNavigationMap()
		{
			// Wait for the first physics frame so the NavigationServer can sync.
			await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
			NavigationMapReady = true;
		}

		public PlayerCharacter Player
		{
			get { return GetNode<PlayerCharacter>("PlayerCharacter"); }
		}

		public HUD Hud
		{
			get { return GetNode<HUD>("HUD"); }
		}
	}
}
