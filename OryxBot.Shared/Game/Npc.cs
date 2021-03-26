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
    }
}
