using System;

namespace OryxBot.Shared.Design
{
    [Serializable]
    public readonly struct Position
    {
        public readonly float X, Y;

        public Position(float x, float y) =>
            (X, Y) = (x, y);

        public override string ToString() {
            return $"X:{X}, Y:{Y}";
        }
    }
}
