using System;
using Godot;

namespace TreeTrunk
{
    public static class NodeExtension
    {
        public static async void RunLater(this Node node, double timeSeconds, Action f)
        {
            await node.ToSignal(node.GetTree().CreateTimer(timeSeconds), SceneTreeTimer.SignalName.Timeout);
            f();
        }
    }
}