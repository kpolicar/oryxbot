using System;
using OryxBot.Shared.Design;
using OryxBot.Shared.Game;
using static OryxBot.Shared.Design.ResponsivePoint;
using static OryxBot.Shared.RunConfiguration;

namespace OryxBot.Client.Linux.Bot
{
    public static class AlbionInterface
    {
        public static ResponsivePoint Character =
            new(1280, 615, 2560, 1440, AnchorStyle.Center);
        
        public static ResponsivePoint FirstItemInInventory =
            new(1590, 550, 1920, 1080, AnchorStyle.Right);
        
        public static ResponsivePoint FirstItemInBank =
            new(132, 515, 3840, 1600, AnchorStyle.Left);
        
        public static ResponsivePoint SecondItemInBank =
            new(255, 515, 3840, 1600, AnchorStyle.Left);
        
        public static ResponsivePoint ThirdItemInBank =
            new(378, 515, 3840, 1600, AnchorStyle.Left);
        
        public static ResponsivePoint IncreaseSplitQuantityButton =
            new(2052, 1060, 3840, 1600, AnchorStyle.Center);
        
        public static ResponsivePoint SplitButton =
            new(2173, 1060, 3840, 1600, AnchorStyle.Center);
        
        public static ResponsivePoint CloseSplitButton =
            new(2260, 460, 3840, 1600, AnchorStyle.Center);
        
        public static ResponsivePoint QuestNpcTradeMissionsTab =
            new(410, 422, 1920, 1080, AnchorStyle.Left);
        
        public static ResponsivePoint QuestNpcTradeMissionsFirstContractTab =
            new(180, 288, 1920, 1080, AnchorStyle.Left);
        
        public static ResponsivePoint QuestNpcTradeMissionsContractTabOffset =
            new(0, 37, 1910, 1080, AnchorStyle.Left);
        
        public static ResponsivePoint QuestNpcSelectFirstTradeMissionContract =
            new(200, 369, 1920, 1080, AnchorStyle.Left);
        
        public static ResponsivePoint QuestNpcSelectTradeMissionContractOffset =
            new(0,98, 1920, 1080, AnchorStyle.Left);

        public static ResponsivePoint QuestNpcOpenTradeMissionContract(City origin, City destination) =>
            CalculateOffsetedPointForTradeMissionContract(QuestNpcSelectFirstTradeMissionContract, origin, destination);

        public static ResponsivePoint QuestNpcSelectTradeMissionContract(City origin, City destination, ContractType contract) {
           var point = CalculateOffsetedPointForTradeMissionContract(
               QuestNpcTradeMissionsFirstContractTab,
               origin,
               destination);
           
           var offset = QuestNpcTradeMissionsContractTabOffset;
           var multiplier = contract switch {
               ContractType.Minor => 0,
               ContractType.Medium => 1,
               ContractType.Major => 2,
               _ => throw new ArgumentOutOfRangeException(nameof(contract), contract, null)
           };
           
           return new (
               point.OriginalX + offset.OriginalX * multiplier,
               point.OriginalY + offset.OriginalY * multiplier,
               offset.OriginalScreenWidth,
               offset.OriginalScreenHeight,
               point.Anchor);
        }

        private static ResponsivePoint CalculateOffsetedPointForTradeMissionContract(
            ResponsivePoint point,
            City origin,
            City destination)
        {
            var offset = QuestNpcSelectTradeMissionContractOffset;
            var multiplier = Cities.FactionLeaderInCityOrderOfMissionForCity(origin, destination);
            
            return new (
                point.OriginalX + offset.OriginalX * multiplier,
                point.OriginalY + offset.OriginalY * multiplier,
                offset.OriginalScreenWidth,
                offset.OriginalScreenHeight,
                point.Anchor);
        }

        public static ResponsivePoint QuestNpcAcceptTradeMissionContract =
            new(1080, 820, 1920, 1080, AnchorStyle.Center);
        
        public static ResponsivePoint QuestNpcProgressContract =
            new(1080, 820, 1920, 1080, AnchorStyle.Center);
        
        public static ResponsivePoint RespawnButton =
            new(1920, 1090, 3840, 1600, AnchorStyle.Center);
    }
}
