using Godot;
using System;

public partial class BulletHurtbox : Area2D, TreeTrunk.IAttack
{
	public float Damage {get; set;} = 0;
}
