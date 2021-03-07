using System;
using System.Threading.Tasks;
using Albion.Network;
using OryxBot.Albion.Protocol;
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

            protected override Task OnActionAsync(TOperation value) {
                return Task.Run(() => CallEvent(value));
            }
                

            protected abstract void CallEvent(TOperation value);
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

            protected override void CallEvent(MoveOperation operation) =>
                DataProvider.Move?.Invoke(this, (MoveEventArgs) operation);
        }

        private class RaiseChangeClusterEvent : RaiseEvent<ChangeClusterOperation>
        {
            public RaiseChangeClusterEvent(NetworkAlbionDataProvider dataProvider) :
                base(dataProvider, (int) OperationCodes.ChangeCluster) {
            }

            protected override void CallEvent(ChangeClusterOperation operation) =>
                DataProvider.ChangeCluster?.Invoke(this, (ChangeClusterEventArgs) operation);
        }

        private class RaiseRegisterToObjectEvent : RaiseEvent
        {
            public RaiseRegisterToObjectEvent(NetworkAlbionDataProvider dataProvider) :
                base(dataProvider, (int) OperationCodes.RegisterToObject) {
            }

            protected override void CallEvent(UnknownOperation value) =>
                DataProvider.RegisterToObject?.Invoke(this, EventArgs.Empty);
        }

        private class RaiseUnRegisterFromObjectEvent : RaiseEvent
        {
            public RaiseUnRegisterFromObjectEvent(NetworkAlbionDataProvider dataProvider) :
                base(dataProvider, (int) OperationCodes.UnRegisterFromObject) {
            }

            protected override void CallEvent(UnknownOperation value) =>
                DataProvider.UnregisterFromObject?.Invoke(this, EventArgs.Empty);
        }

        private class RaiseInventoryMoveItemEvent : RaiseEvent
        {
            public RaiseInventoryMoveItemEvent(NetworkAlbionDataProvider dataProvider) :
                base(dataProvider, (int) OperationCodes.InventoryMoveItem) {
            }

            protected override void CallEvent(UnknownOperation value) =>
                DataProvider.InventoryMoveItem?.Invoke(this, EventArgs.Empty);
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
