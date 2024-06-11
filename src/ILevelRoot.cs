namespace TreeTrunk
{
    interface ILevelRoot
    {
        public abstract PlayerCharacter Player { get; }
        public abstract bool NavigationMapReady { get; }
    }
}