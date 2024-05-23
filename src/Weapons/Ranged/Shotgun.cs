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

    private Bullet BuildBullet(Vector2 bulletDirection)
    {
        var velocity = bulletDirection * 600.0f;
        var bullet = BulletScene.Instantiate<Bullet>();

        bullet.Velocity = velocity.Rotated(Mathf.DegToRad((float)GD.Randfn(0.0, 1.0) * 5));
        bullet.GlobalRotation = velocity.Angle();

        return bullet;
    }

    public override Bullet[] TriggerRangedAttack(Vector2 bulletDirection)
    {
        if (!_canShoot)
        {
            return new Bullet[] {};
        }
        if (CurrentAmmo <= 0)
        {
            return new Bullet[] {};
        }

        _canShoot = false;
        CurrentAmmo -= 1;

        this.RunLater(1 / FireRate, () => _canShoot = true);
        return new Bullet[] { BuildBullet(bulletDirection),
        BuildBullet(bulletDirection),
        BuildBullet(bulletDirection),
        BuildBullet(bulletDirection),
        BuildBullet(bulletDirection),
        BuildBullet(bulletDirection) };

    }

    public override void _Ready()
    {
        base._Ready();
        CurrentAmmo = MaxAmmo;
    }
}
