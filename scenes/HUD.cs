using Godot;

public partial class HUD : CanvasLayer
{
	public GameManager GameManager { get { return GetNode<GameManager>("/root/GameManager"); } }

	public override void _Ready()
	{
		base._Ready();
		GetNode<Label>("AmmoCount").Text = GameManager.PlayerState.CurrentAmmo.ToString();
	}

	public void OnPlayerCharacterCurrentAmmoChanged(int newCurrentAmmoValue)
	{
		GetNode<Label>("AmmoCount").Text = newCurrentAmmoValue.ToString();
	}
}
