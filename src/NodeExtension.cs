using System;
using Godot;

namespace TreeTrunk
{
    public static class NodeExtension
    {

        // The node needs to be added to the scene tree for this to work
        public static async void RunLater(this Node node, double timeSeconds, Action f)
        {
            await node.ToSignal(node.GetTree().CreateTimer(timeSeconds), SceneTreeTimer.SignalName.Timeout);
            f();
        }
    }
}