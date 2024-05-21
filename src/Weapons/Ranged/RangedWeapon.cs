using Godot;
using System;

namespace TreeTrunk
{
    
    public abstract partial class RangedWeapon : Node
    {
        abstract public Bullet[] TriggerRangedAttack(Vector2 bulletDirection);

        private int _currentAmmoValue;

        public int CurrentAmmo
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
    }
}