using Godot;

namespace TreeTrunk
{
	public partial class Railgun : RangedWeapon
	{
		public override double FireRate { get; set; } = 0.5;
		public override int MaxAmmo { get; set; } = 5;
		public override float ReloadDelay { get; set;} = 3.0f;
		public override float ReloadInterval { get; set; } = 1.5f;

		private PackedScene RailgunLaserScene = GD.Load<PackedScene>("res://src/Projectiles/RailgunLaser/RailgunLaser.tscn");

		private RailgunLaser BuildLaser(Vector2 bulletDirection, Vector2 globalSpawnPosition)
		{
			var laser = RailgunLaserScene.Instantiate<RailgunLaser>();
			laser.Position = globalSpawnPosition;
			laser.GlobalRotation = bulletDirection.Angle();

			return laser;
		}

		protected override void SpawnGunfire(Vector2 bulletDirection, Vector2 globalSpawnPosition)
		{
			var gameManager = GetNode<GameManager>("/root/GameManager");

			var laser = BuildLaser(bulletDirection, globalSpawnPosition);
			gameManager.AddToCurrentScene(laser);
			this.RunLater(0.1, laser.RemoveHitboxAndFadeout);
		}
	}
}
