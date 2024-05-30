using Godot;
using System;

namespace TreeTrunk
{
    public abstract partial class RangedWeapon : Node
    {
        abstract public void TriggerRangedAttack(Vector2 bulletDirection, Vector2 globalSpawnPosition);
        abstract public int MaxAmmo { get; set; }

        // The time delay before reloading starts.
        public float ReloadDelay {get; set; } = 1.5f;
        
        // The time taken between ammo ticking up during reload.
        public float ReloadInterval {get; set;} = 0.5f;

        private int _currentAmmoValue;
        private Timer _reloadTimer;

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

        [Signal]
        public delegate void CurrentAmmoChangedEventHandler(int newCurrentAmmoValue);
    }
}