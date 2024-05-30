using Godot;
using TreeTrunk;

public partial class Pistol : RangedWeapon
{

	[Export]
	public PackedScene BulletScene = GD.Load<PackedScene>("res://src/Projectiles/PistolBullet.tscn");

	[Export]
	public double FireRate { get; set; } = 5.0;

	[Export]
	public override int MaxAmmo { get; set; } = 10;

	private bool _canShoot = true;

	private Bullet BuildBullet(Vector2 bulletDirection, Vector2 globalSpawnPosition)
	{
		var velocity = bulletDirection * 600.0f;
		var bullet = BulletScene.Instantiate<Bullet>();

		bullet.Velocity = velocity;
		bullet.GlobalRotation = velocity.Angle();
		bullet.GlobalPosition = globalSpawnPosition;

		return bullet;
	}

	public override void TriggerRangedAttack(Vector2 bulletDirection, Vector2 globalSpawnPosition)
	{
		if (!_canShoot)
		{
			return;
		}
		if (CurrentAmmo <= 0)
		{
			return;
		}

		_canShoot = false;
		CurrentAmmo -= 1;

		this.RunLater(1 / FireRate, () => _canShoot = true);

		var gameManager = GetNode<GameManager>("/root/GameManager");
		gameManager.AddToCurrentScene(BuildBullet(bulletDirection, globalSpawnPosition));
	}

	public override void _Ready()
	{
		base._Ready();
		CurrentAmmo = MaxAmmo;
	}
}
