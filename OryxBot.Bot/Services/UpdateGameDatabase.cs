using System.Threading.Tasks;
using Albion.Network;
using OryxBot.Albion.Protocol;
using OryxBot.Shared.Design;

namespace OryxBot.Bot.Services
{
    public partial class NetworkAlbionDataProvider
    {
        private void BindEventRaiseHandlers(ReceiverBuilder builder) {
            builder.AddRequestHandler(new UpdateCharacterPosition());
            builder.AddRequestHandler(new UpdateCharacterCluster());
            builder.AddRequestHandler(new UpdateCharacterAddInteracting());
            builder.AddRequestHandler(new RaiseUnRegisterFromObjectEvent());
            
            // builder.AddHandler(new AsyncRaiseRequestPacketEvent(this));
            // builder.AddHandler(new AsyncRaiseEventPacketEvent(this));
        }

        private class UpdateCharacterPosition : RequestPacketHandler<MoveOperation>
        {
            public UpdateCharacterPosition() :
                base((int) OperationCodes.Move) {
            }

            protected override Task OnActionAsync(MoveOperation value) {
                Game.LocalCharacter.Instance.Position = new Position(value.Position[0], value.Position[1]);
                return Task.CompletedTask;
            }
        }

        private class UpdateCharacterCluster : RequestPacketHandler<ChangeClusterOperation>
        {
            public UpdateCharacterCluster() :
                base((int) OperationCodes.ChangeCluster) {
            }

            protected override Task OnActionAsync(ChangeClusterOperation value) {
                Game.LocalCharacter.Instance.Cluster = value.Location;
                return Task.CompletedTask;
            }
        }

        private class UpdateCharacterAddInteracting : RequestPacketHandler<UnknownOperation>
        {
            public UpdateCharacterAddInteracting() :
                base((int) OperationCodes.RegisterToObject) {
            }
            
            protected override Task OnActionAsync(UnknownOperation value) {
                Game.LocalCharacter.Instance.Interacting = true;
                return Task.CompletedTask;
            }
        }

        private class RaiseUnRegisterFromObjectEvent : RequestPacketHandler<UnknownOperation>
        {
            public RaiseUnRegisterFromObjectEvent() :
                base((int) OperationCodes.UnRegisterFromObject) {
            }

            protected override Task OnActionAsync(UnknownOperation value) {
                Game.LocalCharacter.Instance.Interacting = false;
                return Task.CompletedTask;
            }
        }
    }
}
