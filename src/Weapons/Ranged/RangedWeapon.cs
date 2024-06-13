using Godot;
using System;

namespace TreeTrunk
{
    public abstract partial class RangedWeapon : Node
    {
        abstract protected void SpawnGunfire(Vector2 bulletDirection, Vector2 globalSpawnPosition);
        abstract public int MaxAmmo { get; set; }
        abstract public double FireRate { get; set; }

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

        public void TriggerRangedAttack(Vector2 bulletDirection, Vector2 globalSpawnPosition)
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

            SpawnGunfire(bulletDirection, globalSpawnPosition);
        }

        [Signal]
        public delegate void CurrentAmmoChangedEventHandler(int newCurrentAmmoValue);
    }
}