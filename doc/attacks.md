# Attacks

* Attacks or hurtboxes are nodes that derive from both `Area2D` and `TreeTrunk.IAttack`.
* Agents that receive damage from attacks should cast the incoming `Area2D` to an `IAttack` to access fields like `Damage`.