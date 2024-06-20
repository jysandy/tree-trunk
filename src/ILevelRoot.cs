namespace TreeTrunk
{
    public interface ILevelRoot
    {
        public abstract PlayerCharacter Player { get; }
        public abstract HUD Hud { get; }
        public abstract bool NavigationMapReady { get; }
    }
}