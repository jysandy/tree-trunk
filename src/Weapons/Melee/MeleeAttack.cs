using Godot;
using TreeTrunk;

public partial class MeleeAttack : Node2D, IAttack
{
	[Export]
	public float Damage { get; set; } = 50;
}
