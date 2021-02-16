using System;
using OryxBot.Shared.Design;

namespace OryxBot.Shared.Events
{
    public class MoveEventArgs : EventArgs
    {
        public Position Position;
        public float Direction;

        public MoveEventArgs(float x, float y) =>
            Position = new Position(x, y);
    }
}
