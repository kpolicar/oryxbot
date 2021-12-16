using System;
using System.Collections.Generic;
using OryxBot.Shared.Design;
using static OryxBot.Shared.Game.City;
using static OryxBot.Shared.Game.Region;

namespace OryxBot.Shared.Game
{
    public static class Npc
    {
        public static class FactionLeader
        {
            public static readonly Dictionary<City, Position> Position = new() {
                { City.Caerleon, new Position(-24.4f, -46.5f) },
                { City.Thetford, new Position(34f, -1f) },
                { City.FortSterling, new Position(-14.25f, -33.77f) },
                { City.Lymhurst, new Position(-75.5f, 0) },
                { City.Bridgewatch, new Position(-19.2f, 35.2f) },
                { City.Martlock, new Position(-9.25f, 45.68f) },
            };
        }

        public static class FactionEmissary
        {
            public static readonly Dictionary<Region, Position> Position = new() {
                { SnapshaftTrough, new Position(70.5f, -240f) }, // Lymhurst->Bridgewatch trade mission
                //{ SnapshaftTrough, new Position(-9.25f, 45.68f) }, // Lymhurst->Bridgewatch trade mission
                { DeadveinGully, new Position(49.17f, -138.86f) }, // Bridgewatch->Caerleon trade mission
                { CairnFidair, new Position(-110.5f, 60.35f) }, // Thetford->FortSterling trade mission
                { BlackthorneQuarry, new Position(338.62f, 260.92f) }, // Caerleon->Martlock trade mission
                { Aspenwood, new Position(159.657f, 40.398f) }, // FortSterling->Lymhurst sterling trade mission
                //{ Aspenwood, new Position(-280.89f, 370.87f) }, // OLD FortSterling->Lymhurst sterling trade mission
                { NightcreakMarsh, new Position( -81.14f, -199.17f) }, // Martlock->Thetford trade mission
                { SleetwaterBasin, new Position( 289.2f, 341f) }, // FortSterling->Thetford trade mission
                { LongtimberGlen, new Position( -80.9f, 210.6f) }, // FortSterling->Caerleon trade mission
                { MalagCrevasse, new Position( 189.1f, -279.4f) }, // FortSterling->Caerleon trade mission
            };

            public static readonly Dictionary<Region, City> Allegiance = new() {
                {SnapshaftTrough, City.Bridgewatch},
                {DeadveinGully, City.Caerleon},
                {CairnFidair, City.FortSterling},
                {BlackthorneQuarry, City.Martlock},
                {Aspenwood, City.Lymhurst},
                {NightcreakMarsh, City.Thetford},
                {SleetwaterBasin, City.Thetford},
                {LongtimberGlen, City.Caerleon},
                {MalagCrevasse, City.Caerleon},
            };
        }
    }
}
