using OryxBot.Shared.Game;

namespace OryxBot.Shared
{
    public readonly struct RecordingConfiguration
    {
        public readonly City Origin { get; }
        public readonly Region Destination { get; }
        public readonly string Name { get; }
        
        public RecordingConfiguration(City origin, Region destination, string name) =>
            (Origin, Destination, Name) = (origin, destination, name);
    }
}
