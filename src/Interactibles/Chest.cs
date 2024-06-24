using Godot;

namespace TreeTrunk
{
	public partial class Chest : StaticBody2D, IInteractible
	{
		[Export]
		public PackedScene WeaponScene { get; set; } = GD.Load<PackedScene>("res://src/Weapons/Ranged/Railgun.tscn");

		private AnimatedSprite2D AnimatedSprite
		{ get { return GetNode<AnimatedSprite2D>("AnimatedSprite2D"); } }

		private GameManager GameManager
		{ get { return GetNode<GameManager>("/root/GameManager"); } }

		private AudioStreamPlayer2D OpenSound
		{ get { return GetNode<AudioStreamPlayer2D>("OpenSound"); } }

		private void Open()
		{
			AnimatedSprite.Play("open");
			OpenSound.Play();
			CollisionLayer = GameManager.BuildPhysicsLayerMask("wall_collisions"); // remove the interactible layer bit
		}

		public void Interact(PlayerCharacter agent)
		{
			Open();

			RangedWeapon newWeapon = WeaponScene.Instantiate<RangedWeapon>();
			agent.AddAndEquipWeapon(newWeapon);
		}
	}
}
