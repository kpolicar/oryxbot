using OryxBot.Shared.Design;
using OryxBot.Shared.Game;
using static OryxBot.Shared.Design.ResponsivePoint;

namespace OryxBot.Client.Windows.Bot
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
        
        public static ResponsivePoint IncreaseSplitQuantityButton =
            new(2052, 1060, 3840, 1600, AnchorStyle.Center);
        
        public static ResponsivePoint SplitButton =
            new(2173, 1060, 3840, 1600, AnchorStyle.Center);
        
        public static ResponsivePoint CloseSplitButton =
            new(2260, 460, 3840, 1600, AnchorStyle.Center);
        
        public static ResponsivePoint QuestNpcTradeMissionsTab =
            new(610, 720, 3840, 1600, AnchorStyle.Left);
        
        public static ResponsivePoint QuestNpcTradeMissionsFirstContractTab =
            new(210, 575, 3840, 1600, AnchorStyle.Left);
        
        public static ResponsivePoint QuestNpcSelectFirstTradeMissionContract =
            new(99,517, 3840, 1600, AnchorStyle.Left);
        
        public static ResponsivePoint QuestNpcSelectTradeMissionContractOffset =
            new(0,55, 3840, 1600, AnchorStyle.Left);

        public static ResponsivePoint QuestNpcOpenTradeMissionContract(City origin, City destination) =>
            CalculateOffsetedPointForTradeMissionContact(QuestNpcSelectFirstTradeMissionContract, origin, destination);

        public static ResponsivePoint QuestNpcSelectTradeMissionContract(City origin, City destination) =>
            CalculateOffsetedPointForTradeMissionContact(QuestNpcTradeMissionsFirstContractTab, origin, destination);

        private static ResponsivePoint CalculateOffsetedPointForTradeMissionContact(
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
                offset.Anchor);
        }

        public static ResponsivePoint QuestNpcAcceptTradeMissionContract =
            new(1080, 820, 1920, 1080, AnchorStyle.Center);
        
        public static ResponsivePoint QuestNpcProgressContract =
            new(1080, 820, 1920, 1080, AnchorStyle.Center);
        
        public static ResponsivePoint RespawnButton =
            new(1920, 1090, 3840, 1600, AnchorStyle.Center);
    }
}
