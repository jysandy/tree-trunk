using System.Diagnostics;
using Godot;

public partial class GameManager : Node
{
    public Root RootNode { get { return GetTree().Root.GetNode<Root>("Root"); } }
    public Node CurrentScene { get { return RootNode.CurrentScene; } }

    public void AddToCurrentScene(Node node)
    {
        CurrentScene.AddChild(node);
    }
}