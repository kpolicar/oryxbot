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
        public event EventHandler<ChangeClusterEventArgs>? ChangeCluster;
        public event EventHandler? RegisterToObject;
        public event EventHandler? UnregisterFromObject;
        public event EventHandler? InventoryMoveItem;
        public event EventHandler<RequestPacket>? NetworkRequest;
        public event EventHandler<EventPacket>? NetworkEvent;
        
        
        public void BindDependencies(ServiceContainer serviceContainer) {
            var bot = serviceContainer.GetService<BotManagerContract>();
            bot.Started += (_, _) => Run();
        }

        private void Run() {
            if (_running)
                return;
            
            var builder = ReceiverBuilder.Create();

            BindEventRaiseHandlers(builder);
            
            _receiver = builder.Build();
    
            foreach (var device in CaptureDeviceList.Instance) {
                var captureThread = new Thread(() => {
                    device.OnPacketArrival += PacketHandler;
                    device.Open(DeviceMode.Promiscuous, 1000);
                    device.Filter = "udp port 5056";
                    
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
