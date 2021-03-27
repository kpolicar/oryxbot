using System;
using System.Collections.Generic;
using System.Linq;

namespace OryxBot.Shared.Game
{
    public enum Region
    {
        Caerleon = City.Caerleon,
        Thetford = City.Thetford,
        FortSterling = City.FortSterling,
        Lymhurst = City.Lymhurst,
        Bridgewatch = City.Bridgewatch,
        Martlock  = City.Martlock,
        
        SnapshaftTrough,
        DeadveinGully,
        CairnFidair,
        BlackthorneQuarry,
        Aspenwood,
        NightcreakMarsh,
    }
    
    public static class Regions
    {
        static Regions() {
            CodeRegionMap = new Dictionary<string, Region>() {
                {"caerleon", Game.Region.Caerleon},
                {"thetford", Game.Region.Thetford},
                {"fort-sterling", Game.Region.FortSterling},
                {"lymhurst", Game.Region.Lymhurst},
                {"bridgewatch", Game.Region.Bridgewatch},
                {"martlock", Game.Region.Martlock},

                {"snapshaft-trough", Game.Region.SnapshaftTrough},
                {"deadvein-gully", Game.Region.DeadveinGully},
                {"cairn-fidair", Game.Region.CairnFidair},
                {"blackthorne-quarry", Game.Region.BlackthorneQuarry},
                {"aspenwood", Game.Region.Aspenwood},
                {"nightcreak-marsh", Game.Region.NightcreakMarsh},
            };
            RegionCodeMap = CodeRegionMap.ToDictionary(
                keyValue => keyValue.Value,
                keyValue => keyValue.Key);
            RegionNameMap = new Dictionary<Region,string>() {
                {Game.Region.Caerleon, "Caerleon"},
                {Game.Region.Thetford, "Thetford"},
                {Game.Region.FortSterling, "FortSterling"},
                {Game.Region.Lymhurst, "Lymhurst"},
                {Game.Region.Bridgewatch, "Bridgewatch"},
                {Game.Region.Martlock, "Martlock"},
                {Game.Region.SnapshaftTrough, "Snapshaft Trough"},
                {Game.Region.DeadveinGully, "Deadvein Gully"},
                {Game.Region.CairnFidair, "Cairn Fidair"},
                {Game.Region.BlackthorneQuarry, "Blackthorne Quarry"},
                {Game.Region.Aspenwood, "Aspenwood"},
                {Game.Region.NightcreakMarsh, "Nightcreak Marsh"},
            };
        }

        public readonly static Dictionary<string, Region> CodeRegionMap;
        public readonly static Dictionary<Region, string> RegionCodeMap;
        public readonly static Dictionary<Region, string> RegionNameMap;

        public static string Name(Region region) =>
            RegionNameMap[region];

        public static Region? Region(string code) =>
            CodeRegionMap[code];

        public static string Code(Region region) =>
            RegionCodeMap[region];
    }
}
