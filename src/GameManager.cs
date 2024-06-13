using System.Collections.Generic;
using System.Linq;
using Godot;
using TreeTrunk;

public partial class GameManager : Node
{
	public Root RootNode
	{
		get
		{
			if (!IsInsideTree()) return null;

			var foo = GetTree().Root;
			if (foo != null)
			{
				return foo.GetNode<Root>("Root");
			}
			else
			{
				return null;
			}
		}
	}
	public Node CurrentScene { get { return RootNode.CurrentScene; } }

	// Physics layer names mapped to layer numbers
	private Dictionary<string, int> _physicsLayers = new Dictionary<string, int>();

	private PlayerState _playerState;

	public PlayerState PlayerState { get { return _playerState; } }

	public PlayerCharacter Player
	{ get { return ((ILevelRoot)CurrentScene).Player; } }

	public bool NavigationMapReady
	{ get { return ((ILevelRoot)CurrentScene).NavigationMapReady; } }

	public void AddToCurrentScene(Node node)
	{
		CurrentScene.AddChild(node);
	}

	public override void _Ready()
	{
		base._Ready();
		_playerState = new PlayerState();
		AddChild(_playerState);

		if (RootNode != null && RootNode.HUD != null)
		{
			PlayerState.CurrentAmmoChanged += RootNode.HUD.OnPlayerCharacterCurrentAmmoChanged;
		}

		for (int i = 1; i <= 32; i++)
		{
			string layerName = (string)ProjectSettings.GetSetting("layer_names/2d_physics/layer_" + i);
			if (!string.IsNullOrEmpty(layerName))
			{
				_physicsLayers[layerName] = i;
			}
		}
	}

	public uint BuildPhysicsLayerMask(string layerName)
	{
		int layerNumber;
		if (!_physicsLayers.TryGetValue(layerName, out layerNumber))
		{
			return 0;
		}

		return (uint)1 << (layerNumber - 1);
	}

	public uint BuildPhysicsLayerMask(IEnumerable<string> layerNames)
	{
		return layerNames.Select(BuildPhysicsLayerMask)
			.Aggregate((mask, next) => mask | next);
	}
}
