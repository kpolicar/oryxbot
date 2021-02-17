using System;
using System.Threading.Tasks;
using Albion.Network;
using OryxBot.Albion.Protocol;

namespace OryxBot.Bot.Services
{
    public partial class NetworkAlbionDataProvider
    {
        internal class NewCharacterEventHandler : EventPacketHandler<NewCharacterEvent>
        {

            public NewCharacterEventHandler() : base((int) EventCodes.NewCharacter) {
            }
            
            protected override Task OnActionAsync(NewCharacterEvent value) {
                Console.WriteLine($"New ch Id: {value.Id}");

                return Task.CompletedTask;
            }
        }
    }
}
