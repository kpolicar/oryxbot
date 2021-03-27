using System;
using System.Collections.Generic;
using System.Linq;

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
        static Cities() {
            CodeCityMap = new Dictionary<string, City> {
                {"caerleon", Game.City.Caerleon},
                {"thetford", Game.City.Thetford},
                {"fort-sterling", Game.City.FortSterling},
                {"lymhurst", Game.City.Lymhurst},
                {"bridgewatch", Game.City.Bridgewatch},
                {"martlock", Game.City.Martlock},
            };
            CityCodeMap = CodeCityMap.ToDictionary(
                keyValue => keyValue.Value,
                keyValue => keyValue.Key);
            CityNameMap = new Dictionary<City, string> {
                {Game.City.Caerleon, "Caerleon"},
                {Game.City.Thetford, "Thetford"},
                {Game.City.FortSterling, "Fort Sterling"},
                {Game.City.Lymhurst, "Lymhurst"},
                {Game.City.Bridgewatch, "Bridgewatch"},
                {Game.City.Martlock, "Martlock"},
            };
        }
        
        public readonly static Dictionary<string, City> CodeCityMap;
        public readonly static Dictionary<City, string> CityCodeMap;
        public readonly static Dictionary<City, string> CityNameMap;

        public static string Name(City city) => CityNameMap[city];
        public static string Code(City city) => CityCodeMap[city];
        public static City City(string code) => CodeCityMap[code];
    }
}
