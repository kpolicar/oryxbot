using System;

namespace OryxBot.Shared.Events
{
    public class MoveEventArgs : EventArgs
    {
        public (float x, float y) Position;
        public float Direction;

        public MoveEventArgs(float x, float y) =>
            Position = (x, y);
    }
}
