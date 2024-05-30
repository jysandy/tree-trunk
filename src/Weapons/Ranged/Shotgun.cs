using Godot;
using TreeTrunk;

public partial class Shotgun : RangedWeapon
{

    [Export]
    public PackedScene BulletScene = GD.Load<PackedScene>("res://src/Projectiles/ShotgunBullet.tscn");

    [Export]
    public double FireRate { get; set; } = 2.0;

    [Export]
    public override int MaxAmmo { get; set; } = 5;

    private bool _canShoot = true;

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
        gameManager.AddToCurrentScene(BuildBullet(bulletDirection, globalSpawnPosition));
        gameManager.AddToCurrentScene(BuildBullet(bulletDirection, globalSpawnPosition));
        gameManager.AddToCurrentScene(BuildBullet(bulletDirection, globalSpawnPosition));
        gameManager.AddToCurrentScene(BuildBullet(bulletDirection, globalSpawnPosition));
        gameManager.AddToCurrentScene(BuildBullet(bulletDirection, globalSpawnPosition));
    }

    public override void _Ready()
    {
        base._Ready();
        CurrentAmmo = MaxAmmo;
    }
}
