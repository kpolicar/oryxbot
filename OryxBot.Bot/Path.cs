using System.Drawing;

namespace OryxBot.Bot
{
    public class Path
    {
        public Point[] waypoints = {
            new Point(165, 79),
            new Point(167, 79),
            new Point(170, 79),
            new Point(172, 79),
            new Point(175, 79),
            new Point(177, 80),
            new Point(180, 80),
            new Point(182, 79),
            new Point(184, 79),
            new Point(186, 80),
            new Point(188, 82),
            new Point(188, 84),
            new Point(188, 87),
            new Point(188, 89),
            new Point(186, 90),
            new Point(183, 90),
            new Point(180, 90),
            new Point(178, 90),
            new Point(175, 91),
            new Point(173, 90),
            new Point(171, 89),
            new Point(170, 87),
            new Point(169, 84),
            new Point(168, 82),
            new Point(168, 79),
        };

        public int currentTargetIndex = 0;
        public Point currentTarget => waypoints[currentTargetIndex];
        public bool isComplete => currentTargetIndex == waypoints.Length;
    }
}
