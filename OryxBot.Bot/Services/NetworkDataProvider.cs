using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Albion.Network;
using OryxBot.Albion.Protocol;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;
using PacketDotNet;
using SharpPcap;
using BotManagerContract=OryxBot.Shared.Contracts.BotManager;

namespace OryxBot.Bot.Services
{
    public partial class NetworkAlbionDataProvider : AlbionDataProvider, IDisposable, HasDependencies
    {
        private IPhotonReceiver _receiver = null!;
        private bool _running;
        
        public event EventHandler<MoveEventArgs>? Move;
        public event EventHandler<RequestPacket>? NetworkRequest;
        public event EventHandler<EventPacket>? NetworkEvent;
        
        
        private class RaiseRequestPacketEvent : PacketHandler<RequestPacket>
        {
            private NetworkAlbionDataProvider DataProvider;

            public RaiseRequestPacketEvent(NetworkAlbionDataProvider dataProvider) =>
                DataProvider = dataProvider;

            protected override Task OnHandleAsync(RequestPacket packet) =>
                new (() => DataProvider.NetworkRequest?.Invoke(this, packet));
        }
        
        private class RaiseEventPacketEvent : PacketHandler<EventPacket>
        {
            private NetworkAlbionDataProvider DataProvider;

            public RaiseEventPacketEvent(NetworkAlbionDataProvider dataProvider) =>
                DataProvider = dataProvider;

            protected override Task OnHandleAsync(EventPacket packet) =>
                new (() => DataProvider.NetworkEvent?.Invoke(this, packet));
        }

        public void BindDependencies(ServiceContainer serviceContainer) {
            var bot = serviceContainer.GetService<BotManagerContract>();
            bot.Started += (_, _) => Run();
        }

        private void Run() {
            if (_running)
                return;
            
            var builder = ReceiverBuilder.Create();
    
            builder.AddRequestHandler(new MoveRequestHandler(this));
            builder.AddHandler(new RaiseRequestPacketEvent(this));
            builder.AddHandler(new RaiseEventPacketEvent(this));
            // builder.AddEventHandler(new MoveEventHandler());
            // builder.AddEventHandler(new NewCharacterEventHandler());
            
            _receiver = builder.Build();
    
            foreach (var device in CaptureDeviceList.Instance) {
                var captureThread = new Thread(() => {
                    Console.WriteLine($"Open... {device.Description}");

                    device.OnPacketArrival += PacketHandler;
                    device.Open(DeviceMode.Promiscuous, 1000);
                    device.StartCapture();
                });
                captureThread.Start();
            }

            _running = true;
        }

        private void Stop() {
            if (!_running)
                return;
            var stopTasks = CaptureDeviceList.Instance.Select(
                device => Task.Run(device.StopCapture));

            Task.WaitAll(stopTasks.ToArray());
            _running = false;
        }
        
        private void PacketHandler(object sender, CaptureEventArgs e)
        {
            UdpPacket packet = Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data).Extract<UdpPacket>();
            if (packet != null && (packet.SourcePort == 5056 || packet.DestinationPort == 5056))
            {
                _receiver.ReceivePacket(packet.PayloadData);
            }
        }

        public void Dispose() =>
            Stop();
    }
}
