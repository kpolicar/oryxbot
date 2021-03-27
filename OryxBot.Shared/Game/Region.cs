using System;

namespace OryxBot.Shared.Game
{
    public enum Region
    {
        // Cities
        Caerleon,
        Thetford,
        FortSterling,
        Lymhurst,
        Bridgewatch,
        Martlock,
        
        SnapshaftTrough,
        DeadveinGully,
        CairnFidair,
        BlackthorneQuarry,
        Aspenwood,
        NightcreakMarsh,
    }
    
    public static class Regions
    {
        public static Region Region(string code) => code switch {
            "caerleon" => Game.Region.Caerleon,
            "thetford" => Game.Region.Thetford,
            "fort-sterling" => Game.Region.FortSterling,
            "lymhurst" => Game.Region.Lymhurst,
            "bridgewatch" => Game.Region.Bridgewatch,
            "martlock" => Game.Region.Martlock,
            
            "snapshaft-trough" => Game.Region.SnapshaftTrough,
            "deadvein-gully" => Game.Region.DeadveinGully,
            "cairn-fidair" => Game.Region.CairnFidair,
            "blackthorne-quarry" => Game.Region.BlackthorneQuarry,
            "aspenwood" => Game.Region.Aspenwood,
            "nightcreak-marsh" => Game.Region.NightcreakMarsh,
            
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, null)
        };
    }
}
