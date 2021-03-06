using OryxBot.Shared.Design;
using static OryxBot.Shared.Design.ResponsivePoint;

namespace OryxBot.Bot
{
    public static class AlbionInterface
    {
        public static ResponsivePoint FirstItemInInventory =
            new(3345, 810, 3840, 1600, AnchorStyle.Right);
        
        public static ResponsivePoint FirstItemInBank =
            new(130, 435, 3840, 1600, AnchorStyle.Left);
        
        public static ResponsivePoint QuestNpcTradeMissionsTab =
            new(605, 700, 3840, 1600, AnchorStyle.Left);
        
        public static ResponsivePoint QuestNpcTradeMissionsContractTab =
            new(490, 500, 3840, 1600, AnchorStyle.Left);
        
        public static ResponsivePoint QuestNpcSelectTradeMissionContract =
            new(225, 560, 3840, 1600, AnchorStyle.Left);
        
        public static ResponsivePoint QuestNpcAcceptTradeMissionContract =
            new(2100, 1210, 3840, 1600, AnchorStyle.Left);
    }
}
