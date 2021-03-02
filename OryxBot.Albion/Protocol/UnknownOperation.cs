using System.Collections.Generic;
using Albion.Network;

namespace OryxBot.Albion.Protocol
{
    public class UnknownOperation : BaseOperation
    {
        public UnknownOperation(Dictionary<byte, object> parameters) : base(parameters) {
        }
    }
}
