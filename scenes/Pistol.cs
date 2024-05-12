using Godot;
using System;
using TreeTrunk;
public partial class Pistol : Node2D
{

	[Export]
	public PackedScene BulletScene;

	[Export]
	public double FireRate { get; set; } = 5.0;

	[Export]
	public int MaxAmmo { get; set; } = 10;

	private bool _canShoot = true;

	private int _currentAmmoValue { get; set; }

	private int CurrentAmmo
	{
		get { return _currentAmmoValue; }
		set
		{
			int newValue = Math.Max(value, 0);

			if (_currentAmmoValue != newValue)
			{
				_currentAmmoValue = newValue;
				EmitSignal(SignalName.CurrentAmmoChanged, newValue);
			}
		}
	}

	[Signal]
	public delegate void CurrentAmmoChangedEventHandler(int newCurrentAmmoValue);

	private Bullet BuildBullet(Vector2 bulletDirection)
	{
		var velocity = bulletDirection * 600.0f;
		var bullet = BulletScene.Instantiate<Bullet>();

		bullet.Velocity = velocity;
		bullet.GlobalRotation = velocity.Angle();

		return bullet;
	}

	public Bullet TriggerRangedAttack(Vector2 bulletDirection)
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
