using OryxBot.Shared.Design;
using static OryxBot.Shared.Design.ResponsivePoint;

namespace OryxBot.Bot
{
    public static class AlbionInterface
    {
        public static ResponsivePoint FirstItemInInventory =
            new(1590, 550, 1920, 1080, AnchorStyle.Right);
        
        public static ResponsivePoint FirstItemInBank =
            new(85, 295, 1920, 1080, AnchorStyle.Left);
        
        public static ResponsivePoint QuestNpcTradeMissionsTab =
            new(415, 470, 1920, 1080, AnchorStyle.Left);
        
        public static ResponsivePoint QuestNpcTradeMissionsContractTab =
            new(155, 335, 1920, 1080, AnchorStyle.Left);
        
        public static ResponsivePoint QuestNpcSelectTradeMissionContract =
            new(150, 375, 1920, 1080, AnchorStyle.Left);
        
        public static ResponsivePoint QuestNpcAcceptTradeMissionContract =
            new(1080, 820, 1920, 1080, AnchorStyle.Left);
        
        public static ResponsivePoint QuestNpcProgressContract =
            new(1080, 820, 1920, 1080, AnchorStyle.Left);
    }
}
