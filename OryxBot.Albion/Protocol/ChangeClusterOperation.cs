using System.Collections.Generic;
using System.Diagnostics;
using Albion.Network;
using OryxBot.Shared.Events;

namespace OryxBot.Albion.Protocol
{
    public class ChangeClusterOperation : BaseOperation
    {
        public string Location { get; }
        
        public ChangeClusterOperation(Dictionary<byte, object> parameters) : base(parameters) {
            Debug.WriteLine("yes");
            Location = parameters[0].ToString();
        }
        
        public static explicit operator ChangeClusterEventArgs(ChangeClusterOperation @this) =>
            new ChangeClusterEventArgs(@this.Location);
    }
}
