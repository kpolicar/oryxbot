using System;
using System.Threading.Tasks;
using Albion.Network;
using OryxBot.Albion.Protocol;

namespace OryxBot.Bot.Services
{
    public partial class NetworkAlbionDataProvider
    {
        internal class MoveEventHandler : EventPacketHandler<MoveEvent>
        {
            public MoveEventHandler() : base(EventCodes.Move) {
            }

            protected override Task OnActionAsync(MoveEvent value) {
                Console.WriteLine($"Id: {value.Id} x: {value.Position[0]} y: {value.Position[1]}");

                return Task.CompletedTask;
            }
        }
    }
}
