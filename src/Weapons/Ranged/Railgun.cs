using Godot;

namespace TreeTrunk
{
	public partial class Railgun : RangedWeapon
	{
		public override double FireRate { get; set; } = 0.5;
		public override int MaxAmmo { get; set; } = 5;
		public override float ReloadDelay { get; set; } = 3.0f;
		public override float ReloadInterval { get; set; } = 1.5f;
		public override Sprite2D Sprite
		{ get { return GetNode<Sprite2D>("Sprite2D"); } }

		public override Marker2D GunfireSpawn
		{ get { return GetNode<Marker2D>("GunfireSpawn"); } }

		private PackedScene RailgunLaserScene = GD.Load<PackedScene>("res://src/Projectiles/RailgunLaser/RailgunLaser.tscn");

		private AudioStreamPlayer2D FireSound
		{ get { return GetNode<AudioStreamPlayer2D>("FireSound"); } }

		private RailgunLaser BuildLaser(Vector2 bulletDirection, Vector2 globalSpawnPosition)
		{
			var laser = RailgunLaserScene.Instantiate<RailgunLaser>();
			laser.GlobalPosition = globalSpawnPosition;
			laser.GlobalRotation = bulletDirection.Angle();

			return laser;
		}

		protected override void SpawnGunfire(Vector2 bulletDirection, Vector2 globalSpawnPosition)
		{
			var gameManager = GetNode<GameManager>("/root/GameManager");

			var laser = BuildLaser(bulletDirection, globalSpawnPosition);
			gameManager.AddToCurrentScene(laser);
			FireSound.Play(0.2f);
			this.RunLater(0.1, laser.RemoveHitboxAndFadeout);
		}
	}
}
