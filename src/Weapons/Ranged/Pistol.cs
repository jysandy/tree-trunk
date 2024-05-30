using Godot;
using TreeTrunk;

public partial class Pistol : RangedWeapon
{

	[Export]
	public PackedScene BulletScene = GD.Load<PackedScene>("res://src/Projectiles/PistolBullet.tscn");

	[Export]
	public override double FireRate { get; set; } = 5.0;

	[Export]
	public override int MaxAmmo { get; set; } = 10;

	private Bullet BuildBullet(Vector2 bulletDirection, Vector2 globalSpawnPosition)
	{
		var velocity = bulletDirection * 600.0f;
		var bullet = BulletScene.Instantiate<Bullet>();

		bullet.Velocity = velocity;
		bullet.GlobalRotation = velocity.Angle();
		bullet.GlobalPosition = globalSpawnPosition;

		return bullet;
	}

	protected override void SpawnGunfire(Vector2 bulletDirection, Vector2 globalSpawnPosition)
	{
		var gameManager = GetNode<GameManager>("/root/GameManager");
		gameManager.AddToCurrentScene(BuildBullet(bulletDirection, globalSpawnPosition));
	}
}
