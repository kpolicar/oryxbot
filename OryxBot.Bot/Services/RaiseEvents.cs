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
            builder.AddRequestHandler(new RaiseRegisterToObjectEvent(this));
            builder.AddRequestHandler(new RaiseUnRegisterFromObjectEvent(this));
            
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

        private abstract class RaiseEvent : RaiseEvent<UnknownOperation>
        {
            protected RaiseEvent(NetworkAlbionDataProvider dataProvider, int operationCode) : base(dataProvider, operationCode) {
            }
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
                DataProvider.ChangeCluster?.Invoke(this, (ChangeClusterEventArgs) operation);
                return Task.CompletedTask;
            }
        }

        private class RaiseRegisterToObjectEvent : RaiseEvent
        {
            public RaiseRegisterToObjectEvent(NetworkAlbionDataProvider dataProvider) :
                base(dataProvider, (int) OperationCodes.RegisterToObject) {
            }

            protected override Task OnActionAsync(UnknownOperation operation) {
                Debug.WriteLine("registered event!");
                DataProvider.RegisterToObject?.Invoke(this, EventArgs.Empty);
                return Task.CompletedTask;
            }
        }

        private class RaiseUnRegisterFromObjectEvent : RaiseEvent
        {
            public RaiseUnRegisterFromObjectEvent(NetworkAlbionDataProvider dataProvider) :
                base(dataProvider, (int) OperationCodes.UnRegisterFromObject) {
            }

            protected override Task OnActionAsync(UnknownOperation operation) {
                Debug.WriteLine("unregistered event!");
                DataProvider.UnregisterFromObject?.Invoke(this, EventArgs.Empty);
                return Task.CompletedTask;
            }
        }

        private class RaiseInventoryMoveItemEvent : RaiseEvent<ChangeClusterOperation>
        {
            public RaiseInventoryMoveItemEvent(NetworkAlbionDataProvider dataProvider) :
                base(dataProvider, (int) OperationCodes.InventoryMoveItem) {
            }

            protected override Task OnActionAsync(ChangeClusterOperation operation) {
                DataProvider.InventoryMoveItem?.Invoke(this, (ChangeClusterEventArgs) operation);
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
