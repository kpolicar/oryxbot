using System;
using OryxBot.Shared.Design;

namespace OryxBot.Shared.Game
{
    public static class Npc
    {
        public static class FactionLeader
        {
            public static Position Position(City city) => city switch {
                City.Caerleon => new Position(-24.4f, -46.5f),
                City.Thetford => new Position(34f, -1f),
                City.FortSterling => new Position(-14.25f, -33.77f),
                City.Lymhurst => new Position(-75.5f, 0),
                City.Bridgewatch => new Position(-19.2f, 35.2f),
                City.Martlock => new Position(-9.25f, 45.68f),
                _ => throw new ArgumentOutOfRangeException(nameof(city), city, null)
            };
        }

        public static class FactionDiplomat
        {
            public static Position Position(string map) => map switch {
                "Snapshaft Trough" => new Position(-9.25f, 45.68f), // Lymhurst trade mission
                "Deadvein Gully" => new Position(49.17f, -138.86f), // Bridgewatch trade mission
                "Cairn Fidair" => new Position(-110.5f, 60.35f), // Thetford trade mission
                "Blackthorne Quarry" => new Position(338.62f, 260.92f), // Caerleon trade mission
                "Aspenwood" => new Position(-280.89f, 370.87f), // Fort sterling trade mission
                _ => throw new ArgumentOutOfRangeException(nameof(map), map, null)
            };
        }
    }
}
