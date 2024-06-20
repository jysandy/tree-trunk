using Godot;

namespace TreeTrunk
{
	public partial class HUD : CanvasLayer
	{
		public GameManager GameManager { get { return GetNode<GameManager>("/root/GameManager"); } }

		public override void _Ready()
		{
			base._Ready();
			SetAmmoCount(GameManager.Player.CurrentAmmo);
		}

		public void SetAmmoCount( int ammoCount)
		{
			GetNode<Label>("AmmoCount").Text = GameManager.Player.CurrentAmmo.ToString();
		}

		public void OnPlayerCharacterCurrentAmmoChanged(int newCurrentAmmoValue)
		{
			GetNode<Label>("AmmoCount").Text = newCurrentAmmoValue.ToString();
		}
	}
}
