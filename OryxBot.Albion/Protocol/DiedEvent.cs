using System.Collections.Generic;
using Albion.Network;
using OryxBot.Shared.Events;

namespace OryxBot.Albion.Protocol
{
    public class DiedEvent : BaseEvent
    {
        public DiedEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
        }
    }
}
