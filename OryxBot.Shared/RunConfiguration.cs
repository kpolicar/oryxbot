
using OryxBot.Shared.Game;

namespace OryxBot.Shared
{
    public readonly struct RunConfiguration
    {
        public enum HeartsType {
            Small=3, Average=7, Large=15
        }
        
        public readonly HeartsType Hearts { get; }
        
        public RunConfiguration(HeartsType type) =>
            (Hearts) = (type);
    }
}
