using System;
using System.Collections.Generic;
using Albion.Network;
using OryxBot.Shared.Events;

namespace OryxBot.Albion.Protocol
{
    public class MoveOperation : BaseOperation
    {
        public MoveOperation(Dictionary<byte, object> parameters) : base(parameters)
        {
            //Time = (int) parameters[0];
            Position = (float[]) parameters[1];
            // Direction = (float)parameters[2];
            // NewPosition = (float[])parameters[3];
            // Speed = (float)parameters[4];
            Console.WriteLine("Moved operation!"+Position[0]+", "+Position[1]);
        }

        // public int Time { get; }
        public float[] Position { get; }
        // public float Direction { get; }
        // public float[] NewPosition { get; }
        // public float Speed { get; }
        
        public static explicit operator MoveEventArgs(MoveOperation @this) =>
            new(@this.Position[0], @this.Position[1]);
    }
}
