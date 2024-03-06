using Godot;
using System;

public partial class HUD : CanvasLayer
{
	private void OnPlayerCharacterCurrentAmmoChanged(long newCurrentAmmoValue)
	{
		GetNode<Label>("AmmoCount").Text = newCurrentAmmoValue.ToString();
	}
}
