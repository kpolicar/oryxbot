using System;
using System.Drawing;
using OryxBot.Shared.Design;
using SMath=System.Math;

namespace OryxBot.Bot
{
    public static class Helpers
    {
        public static class Math
        {
            public static double Angle(Position vA, Position vB) {
                var origin = new Position(0, 0);
                var magA = Distance(vA, origin);
                var magB = Distance(vB, origin);
                var AB = vA.X * vB.X + vA.Y * vB.Y;
                return SMath.Acos(AB / (magA * magB));
            }

            public static double Distance(Position A, Position B) =>
                SMath.Sqrt(
                    SMath.Pow(B.X - A.X, 2) + SMath.Pow(B.Y - A.Y, 2));

            public static (double x, double y) PointOnUnitCircle(double angle) =>
                (SMath.Cos(angle), SMath.Sin(angle));
        }
    }
}
