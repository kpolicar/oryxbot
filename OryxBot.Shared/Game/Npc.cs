using System;
using System.Collections.Generic;
using OryxBot.Shared.Design;

namespace OryxBot.Shared.Game
{
    public static class Npc
    {
        public static class FactionLeader
        {
            public static Dictionary<City, Position> Position = new() {
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
            public static Dictionary<Region, Position> Position = new() {
                { Region.SnapshaftTrough, new Position(-9.25f, 45.68f) }, // Lymhurst trade mission
                { Region.DeadveinGully, new Position(49.17f, -138.86f) }, // Bridgewatch trade mission
                { Region.CairnFidair, new Position(-110.5f, 60.35f) }, // Thetford trade mission
                { Region.BlackthorneQuarry, new Position(338.62f, 260.92f) }, // Caerleon trade mission
                { Region.Aspenwood, new Position(-280.89f, 370.87f) }, // Fort sterling trade mission
                { Region.NightcreakMarsh, new Position( -81.14f, -199.17f) }, // Martlock trade mission
            };
        }
    }
}
