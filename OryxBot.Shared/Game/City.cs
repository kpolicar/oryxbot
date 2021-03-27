using System;

namespace OryxBot.Shared.Game
{
    public enum City
    {
        Caerleon,
        Thetford,
        FortSterling,
        Lymhurst,
        Bridgewatch,
        Martlock
    }

    public static class Cities
    {
        public static string Name(City city) => city switch {
            Game.City.Caerleon => "Caerleon",
            Game.City.Thetford => "Thetford",
            Game.City.FortSterling => "Fort Sterling",
            Game.City.Lymhurst => "Lymhurst",
            Game.City.Bridgewatch => "Bridgewatch",
            Game.City.Martlock => "Martlock",
            _ => throw new ArgumentOutOfRangeException(nameof(city), city, null)
        };
        
        public static City City(string name) => name switch {
            "Caerleon" => Game.City.Caerleon,
            "Thetford" => Game.City.Thetford,
            "Fort Sterling" => Game.City.FortSterling,
            "Lymhurst" => Game.City.Lymhurst,
            "Bridgewatch" => Game.City.Bridgewatch,
            "Martlock" => Game.City.Martlock,
            _ => throw new ArgumentOutOfRangeException(nameof(name), name, null)
        };
    }
}
