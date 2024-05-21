# Attacks

* Attacks are nodes that contain an `Area2D` and implement `IAttack`.
* Agents that receive damage from attacks should cast the parent of the incoming `Area2D` to an `IAttack` to access fields like `Damage`.