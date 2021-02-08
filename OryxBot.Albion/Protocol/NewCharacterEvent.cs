using System.Collections.Generic;
using Albion.Network;

namespace OryxBot.Albion.Protocol
{
    public class NewCharacterEvent : BaseEvent
    {
        public NewCharacterEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            Id = parameters[0].ToString();
            Name = parameters[1].ToString();
            GuildName = parameters.TryGetValue(8, out object guildName) ? guildName.ToString() : null;
            Position = (float[])parameters[12];
        }

        public string Id { get; }
        public string Name { get; }
        public string GuildName { get; }
        public float[] Position { get; }
    }
}
