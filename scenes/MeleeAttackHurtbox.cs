using Godot;
using System;
using System.Diagnostics;

public partial class MeleeAttackHurtbox : Area2D, TreeTrunk.IAttack
{
	[Export]
	public float Damage {get; set;} = 50;
}
