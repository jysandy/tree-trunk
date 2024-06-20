using Godot;

namespace TreeTrunk
{
    public partial class PlayerState : Node
    {
        [Export]
        public float Speed { get; set; } = 200.0f;

        [Export]
        public float MaxHealth { get; set; } = 10000.0f;

        private float _currentHealth;
        private RangedWeapon[] _weapons;
        private int _equippedWeaponIndex = 0;

        [Signal]
        public delegate void CurrentAmmoChangedEventHandler(int newCurrentAmmoValue);

        public RangedWeapon CurrentWeapon { get { return _weapons[_equippedWeaponIndex]; } }

        public int CurrentAmmo { get { return CurrentWeapon.CurrentAmmo; } }

        public bool IsDead
        {
            get { return _currentHealth <= 0; }
        }

        private void EmitCurrentAmmoChanged()
        {
            EmitSignal(SignalName.CurrentAmmoChanged, CurrentWeapon.CurrentAmmo);
        }

        public void CycleWeapon()
        {
            _equippedWeaponIndex = (_equippedWeaponIndex + 1) % _weapons.Length;
            EmitCurrentAmmoChanged();
        }

        public void TakeDamage(float damage)
        {
            _currentHealth = Mathf.Clamp(_currentHealth - damage, 0, MaxHealth);
        }

        public void TriggerRangedAttack(Vector2 bulletDirection, Vector2 spawnPosition)
        {
            CurrentWeapon.TriggerRangedAttack(bulletDirection, spawnPosition);
        }

        // Weapons have to be scenes in order to contain AudioStreamPlayers with WAVs.
        private PackedScene _pistolScene = GD.Load<PackedScene>("res://src/Weapons/Ranged/Pistol.tscn");
        private PackedScene _railgunScene = GD.Load<PackedScene>("res://src/Weapons/Ranged/Railgun.tscn");

        public override void _Ready()
        {
            base._Ready();
            _currentHealth = MaxHealth;
            _weapons = new RangedWeapon[] { 
                _pistolScene.Instantiate<Pistol>(), 
                _railgunScene.Instantiate<Railgun>()
             };
            foreach (var weapon in _weapons)
            {
                AddChild(weapon);
                weapon.Connect(RangedWeapon.SignalName.CurrentAmmoChanged,
                    Callable.From<int>((_) => EmitCurrentAmmoChanged()));
            }
            EmitCurrentAmmoChanged();
        }
    }
}
