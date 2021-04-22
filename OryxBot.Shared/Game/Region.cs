using System;
using System.Collections.Generic;
using System.Linq;
using static OryxBot.Shared.Game.Region;

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
        SleetwaterBasin,
        LongtimberGlen,
        MalagCrevasse,
    }
    
    public static class Regions
    {
        static Regions() {
            CodeRegionMap = new Dictionary<string, Region>() {
                {"caerleon", Caerleon},
                {"thetford", Thetford},
                {"fort-sterling", FortSterling},
                {"lymhurst", Lymhurst},
                {"bridgewatch", Bridgewatch},
                {"martlock", Martlock},

                {"snapshaft-trough", SnapshaftTrough},
                {"deadvein-gully", DeadveinGully},
                {"cairn-fidair", CairnFidair},
                {"blackthorne-quarry", BlackthorneQuarry},
                {"aspenwood", Aspenwood},
                {"nightcreak-marsh", NightcreakMarsh},
                {"sleetwater-basin", SleetwaterBasin},
                {"longtimber-glen", LongtimberGlen},
                {"malag-crevasse", MalagCrevasse},
            };
            RegionCodeMap = CodeRegionMap.ToDictionary(
                keyValue => keyValue.Value,
                keyValue => keyValue.Key);
            RegionNameMap = new Dictionary<Region,string>() {
                {Caerleon, "Caerleon"},
                {Thetford, "Thetford"},
                {FortSterling, "FortSterling"},
                {Lymhurst, "Lymhurst"},
                {Bridgewatch, "Bridgewatch"},
                {Martlock, "Martlock"},
                {SnapshaftTrough, "Snapshaft Trough"},
                {DeadveinGully, "Deadvein Gully"},
                {CairnFidair, "Cairn Fidair"},
                {BlackthorneQuarry, "Blackthorne Quarry"},
                {Aspenwood, "Aspenwood"},
                {NightcreakMarsh, "Nightcreak Marsh"},
                {SleetwaterBasin, "Sleetwater Basin"},
                {LongtimberGlen, "Longtimber Glen"},
                {MalagCrevasse, "Malag Crevasse"},
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
