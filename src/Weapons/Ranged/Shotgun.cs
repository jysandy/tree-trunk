using Godot;

namespace TreeTrunk
{
    public partial class Shotgun : RangedWeapon
    {

        [Export]
        public PackedScene BulletScene = GD.Load<PackedScene>("res://src/Projectiles/ShotgunBullet.tscn");

        [Export]
        public override double FireRate { get; set; } = 2.0;

        [Export]
        public override int MaxAmmo { get; set; } = 5;

        private Bullet BuildBullet(Vector2 bulletDirection, Vector2 globalSpawnPosition)
        {
            var velocity = bulletDirection * 600.0f;
            var bullet = BulletScene.Instantiate<Bullet>();

            // Add some spread to the shotgun pellets.
            bullet.Velocity = velocity.Rotated(Mathf.DegToRad((float)GD.Randfn(0.0, 1.0) * 5));
            bullet.GlobalRotation = velocity.Angle();

            bullet.GlobalPosition = globalSpawnPosition;

            return bullet;
        }

        protected override void SpawnGunfire(Vector2 bulletDirection, Vector2 globalSpawnPosition)
        {
            var gameManager = GetNode<GameManager>("/root/GameManager");

            gameManager.AddToCurrentScene(BuildBullet(bulletDirection, globalSpawnPosition));
            gameManager.AddToCurrentScene(BuildBullet(bulletDirection, globalSpawnPosition));
            gameManager.AddToCurrentScene(BuildBullet(bulletDirection, globalSpawnPosition));
            gameManager.AddToCurrentScene(BuildBullet(bulletDirection, globalSpawnPosition));
            gameManager.AddToCurrentScene(BuildBullet(bulletDirection, globalSpawnPosition));
            gameManager.AddToCurrentScene(BuildBullet(bulletDirection, globalSpawnPosition));
        }
    }
}
