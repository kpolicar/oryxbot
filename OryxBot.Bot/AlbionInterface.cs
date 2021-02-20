using OryxBot.Shared.Design;

namespace OryxBot.Bot
{
    public static class AlbionInterface
    {
        public static ResponsivePoint FirstItemInInventory =
            new(3345, 810, 3840, 1600);
        
        public static ResponsivePoint FirstItemInBank =
            new(130, 435, 3840, 1600);
        
        public static ResponsivePoint QuestNpcTradeMissionsTab =
            new(605, 700, 3840, 1600);
        
        public static ResponsivePoint QuestNpcTradeMissionsContractTab =
            new(490, 500, 3840, 1600);
        
        public static ResponsivePoint QuestNpcSelectTradeMissionContract =
            new(225, 560, 3840, 1600);
        
        public static ResponsivePoint QuestNpcAcceptTradeMissionContract =
            new(2100, 1210, 3840, 1600);
    }
}
