using Godot;
using System;
using System.Diagnostics;

namespace TreeTrunk
{
    public abstract partial class RangedWeapon : Node2D
    {
        abstract protected void SpawnGunfire(Vector2 bulletDirection, Vector2 globalSpawnPosition);
        abstract public int MaxAmmo { get; set; }
        abstract public double FireRate { get; set; }

        abstract public Sprite2D Sprite { get; }
        abstract public Marker2D GunfireSpawn { get; }

        private Vector2 _originalSpritePosition;

        // The time delay before reloading starts.
        public virtual float ReloadDelay { get; set; } = 1.5f;

        // The time taken between ammo ticking up during reload.
        public virtual float ReloadInterval { get; set; } = 0.5f;

        private int _currentAmmoValue;
        private Timer _reloadTimer;
        private bool _canShoot = true;

        public bool IsAmmoFull { get { return CurrentAmmo == MaxAmmo; } }

        public int CurrentAmmo
        {
            get { return _currentAmmoValue; }
            set
            {
                int newValue = Math.Max(value, 0);
                var oldValue = _currentAmmoValue;

                if (_currentAmmoValue != newValue)
                {
                    _currentAmmoValue = newValue;
                    EmitSignal(SignalName.CurrentAmmoChanged, newValue);
                }

                if (_currentAmmoValue < oldValue)
                {
                    _reloadTimer.Stop();
                    _reloadTimer.Start(ReloadDelay);
                }
            }
        }

        public virtual void Equip()
        {
            Visible = true;
        }

        public virtual void Unequip()
        {
            Visible = false;
        }

        public override void _Ready()
        {
            base._Ready();
            CurrentAmmo = MaxAmmo;
            _reloadTimer = new Timer
            {
                Autostart = false,
                OneShot = true
            };
            _reloadTimer.Timeout += OnReloadTimerTimeout;

            AddChild(_reloadTimer);

            _originalSpritePosition = Sprite.Position;
        }

        public override void _Process(double delta)
        {
            base._Process(delta);
            LookAtMouse();
        }

        private void LookAtMouse()
        {
            var toMouse = GetGlobalMousePosition() - GlobalPosition;
            GlobalRotation = toMouse.Angle();
            Sprite.FlipV = toMouse.X < 0;

            if (toMouse.X < 0)
            {
                Sprite.FlipV = true;
                // Mirror about the GunfireSpawn, not the weapon origin.
                Sprite.Position = new Vector2(
                    _originalSpritePosition.X,
                    2 * GunfireSpawn.Position.Y - _originalSpritePosition.Y
                );
            }
            else
            {
                Sprite.FlipV = false;
                Sprite.Position = _originalSpritePosition;                
            }
        }

        private void OnReloadTimerTimeout()
        {
            if (IsAmmoFull)
            {
                return;
            }

            CurrentAmmo += 1;
            _reloadTimer.Start(ReloadInterval);
        }

        public void TriggerRangedAttack()
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

            var spawnPosition = GunfireSpawn.GlobalPosition;
            var bulletDirection = (GetGlobalMousePosition() - spawnPosition).Normalized();
            SpawnGunfire(bulletDirection, spawnPosition);
        }

        [Signal]
        public delegate void CurrentAmmoChangedEventHandler(int newCurrentAmmoValue);
    }
}