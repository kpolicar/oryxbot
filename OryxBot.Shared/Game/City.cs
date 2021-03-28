using System;
using System.Collections.Generic;
using System.Linq;
using static OryxBot.Shared.Game.City;

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
                {"caerleon", Caerleon},
                {"thetford", Thetford},
                {"fort-sterling", FortSterling},
                {"lymhurst", Lymhurst},
                {"bridgewatch", Bridgewatch},
                {"martlock", Martlock},
            };
            CityCodeMap = CodeCityMap.ToDictionary(
                keyValue => keyValue.Value,
                keyValue => keyValue.Key);
            CityNameMap = new Dictionary<City, string> {
                {Caerleon, "Caerleon"},
                {Thetford, "Thetford"},
                {FortSterling, "Fort Sterling"},
                {Lymhurst, "Lymhurst"},
                {Bridgewatch, "Bridgewatch"},
                {Martlock, "Martlock"},
            };
        }
        
        public readonly static Dictionary<string, City> CodeCityMap;
        public readonly static Dictionary<City, string> CityCodeMap;
        public readonly static Dictionary<City, string> CityNameMap;

        public static string Name(City city) => CityNameMap[city];
        public static string Code(City city) => CityCodeMap[city];
        public static City City(string code) => CodeCityMap[code];
        
        public readonly static Dictionary<City, City[]> FactionLeaderCityMissionOrdering = new() {
            {Caerleon, new[] {Thetford, Lymhurst, FortSterling, Martlock, Bridgewatch}},
            {Bridgewatch, new[] {Caerleon, Lymhurst, Martlock, FortSterling, Thetford}},
            {FortSterling, new[] {Caerleon,Thetford,Lymhurst,Bridgewatch,Martlock}},
            {Lymhurst, new[] {Caerleon,Bridgewatch,FortSterling,Thetford,Martlock}},
            {Martlock, new[] {Caerleon,Bridgewatch,Thetford,FortSterling,Lymhurst}},
            {Thetford, new[] {Caerleon,Martlock,FortSterling,Lymhurst,Bridgewatch}},
        };

        public static int FactionLeaderInCityOrderOfMissionForCity(City origin, City destination) =>
            Array.IndexOf(FactionLeaderCityMissionOrdering[origin], destination);
    }
}
