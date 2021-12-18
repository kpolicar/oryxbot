using System.Collections.Generic;
using Albion.Network;
using OryxBot.Shared.Events;

namespace OryxBot.Albion.Protocol
{
    public class ChangeClusterOperation : BaseOperation
    {
        public string Location { get; }
        
        public ChangeClusterOperation(Dictionary<byte, object> parameters) : base(parameters) {
            Location = parameters[0].ToString();
            //Map = parameters[1].ToString();
            //Owner = parameters[2].ToString();
        }
        
        public static explicit operator ChangeClusterEventArgs(ChangeClusterOperation @this) =>
            new ChangeClusterEventArgs(@this.Location);
    }
}
