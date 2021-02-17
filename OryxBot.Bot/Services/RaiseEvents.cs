using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
using Albion.Network;
using OryxBot.Albion.Protocol;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Events;

namespace OryxBot.Bot.Services
{
    public partial class NetworkAlbionDataProvider
    {
        private void BindEventRaiseHandlers(ReceiverBuilder builder) {
            builder.AddRequestHandler(new RaiseMoveEvent(this));
            builder.AddRequestHandler(new RaiseChangeClusterEvent(this));
            
            // builder.AddHandler(new AsyncRaiseRequestPacketEvent(this));
            // builder.AddHandler(new AsyncRaiseEventPacketEvent(this));
        }

        private abstract class RaiseEvent<TOperation> : RequestPacketHandler<TOperation>
            where TOperation : BaseOperation
        {
            protected readonly NetworkAlbionDataProvider DataProvider;
            
            protected RaiseEvent(NetworkAlbionDataProvider dataProvider, int operationCode) : base(operationCode) =>
                DataProvider = dataProvider;
        }

        private class RaiseMoveEvent : RaiseEvent<MoveOperation>
        {
            public RaiseMoveEvent(NetworkAlbionDataProvider dataProvider) :
                base(dataProvider, (int) OperationCodes.Move) {
            }

            protected override Task OnActionAsync(MoveOperation operation) {
                DataProvider.Move?.Invoke(this, (MoveEventArgs) operation);
                return Task.CompletedTask;
            }
        }

        private class RaiseChangeClusterEvent : RaiseEvent<ChangeClusterOperation>
        {
            public RaiseChangeClusterEvent(NetworkAlbionDataProvider dataProvider) :
                base(dataProvider, (int) OperationCodes.ChangeCluster) {
            }

            protected override Task OnActionAsync(ChangeClusterOperation operation) {
                Debug.WriteLine("changed cluster");
                DataProvider.ChangeCluster?.Invoke(this, (ChangeClusterEventArgs) operation);
                return Task.CompletedTask;
            }
        }
        
        private class AsyncRaiseRequestPacketEvent : PacketHandler<RequestPacket>
        {
            private NetworkAlbionDataProvider DataProvider;

            public AsyncRaiseRequestPacketEvent(NetworkAlbionDataProvider dataProvider) =>
                DataProvider = dataProvider;

            protected override Task OnHandleAsync(RequestPacket packet) =>
                Task.Run(() => DataProvider.NetworkRequest?.Invoke(this, packet));
        }
        
        private class AsyncRaiseEventPacketEvent : PacketHandler<EventPacket>
        {
            private NetworkAlbionDataProvider DataProvider;

            public AsyncRaiseEventPacketEvent(NetworkAlbionDataProvider dataProvider) =>
                DataProvider = dataProvider;

            protected override Task OnHandleAsync(EventPacket packet) =>
                Task.Run(() => DataProvider.NetworkEvent?.Invoke(this, packet));
        }
    }
}
