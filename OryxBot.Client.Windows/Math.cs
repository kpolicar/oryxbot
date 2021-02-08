using System.Drawing;
using System.Numerics;

namespace OryxBot.Client.Windows
{
    public static class Math
    {
        public static float Lerp(float firstFloat, float secondFloat, float by)
        {
            return firstFloat * (1 - by) + secondFloat * by;
        }
        
        public static Point Lerp(Point firstVector, Point secondVector, float by)
        {
            var retX = Lerp(firstVector.X, secondVector.X, by);
            var retY = Lerp(firstVector.Y, secondVector.Y, by);
            return new Point((int)retX, (int)retY);
        }
    }
}
