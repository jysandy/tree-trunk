using Godot;
using TreeTrunk;

public partial class Pistol : RangedWeapon
{

	[Export]
	public PackedScene BulletScene = GD.Load<PackedScene>("res://src/Projectiles/PistolBullet.tscn");

	[Export]
	public double FireRate { get; set; } = 5.0;

	[Export]
	public int MaxAmmo { get; set; } = 100;

	private bool _canShoot = true;

	private Bullet BuildBullet(Vector2 bulletDirection)
	{
		var velocity = bulletDirection * 600.0f;
		var bullet = BulletScene.Instantiate<Bullet>();

		bullet.Velocity = velocity;
		bullet.GlobalRotation = velocity.Angle();

		return bullet;
	}

	public override Bullet TriggerRangedAttack(Vector2 bulletDirection)
	{
		if (!_canShoot)
		{
			return null;
		}
		if (CurrentAmmo <= 0)
		{
			return null;
		}

		_canShoot = false;
		CurrentAmmo -= 1;

		this.RunLater(1 / FireRate, () => _canShoot = true);
		return BuildBullet(bulletDirection);

	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CurrentAmmo = MaxAmmo;
	}
}
