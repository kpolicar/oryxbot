using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Albion.Network;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Game;
using PacketDotNet;
using SharpPcap;
using BotManagerContract=OryxBot.Shared.Contracts.BotManager;

namespace OryxBot.Client.Linux.Bot.Services
{
    public partial class NetworkAlbionDataProvider : AlbionDataProvider, IDisposable, HasDependencies
    {
        internal static readonly int QueryNetworkInterval = 5;
        
        
        private IPhotonReceiver _receiver = null!;
        private bool _running;
        
        public event EventHandler<RequestPacket>? NetworkRequest;
        public event EventHandler<EventPacket>? NetworkEvent;
        
        
        public void BindDependencies(ServiceContainer serviceContainer) {
            var bot = serviceContainer.GetService<BotManagerContract>();
        }

        public void Run() {
            if (_running)
                return;
            
            var builder = ReceiverBuilder.Create();

            BindEventRaiseHandlers(builder);
            
            _receiver = builder.Build();

            var ports = new[] { 5056, 5055, 4535 };
            foreach (var device in CaptureDeviceList.Instance) {
                if (!device.Name.StartsWith("eth"))
                    return;
                var captureThread = new Thread(() => {
                    device.OnPacketArrival += PacketHandler;
                    device.Open(DeviceModes.Promiscuous, QueryNetworkInterval);
                    
                    var portsFilter = "port " + string.Join(" or port ", ports);
                    device.Filter = $"ip and udp and ({portsFilter})";
                    
                    if (device.LinkType != LinkLayers.Ethernet) {
                        device.Close();
                        return;
                    }
                    
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
                device => Task.Run(() => {
                    device.StopCapture();
                    device.Close();
                }));

            Task.WaitAll(stopTasks.ToArray());
            _running = false;
        }
        
        private void PacketHandler(object sender, PacketCapture e)
        {
            try {
                var capture = e.GetPacket();
                UdpPacket packet = Packet.ParsePacket(capture.LinkLayerType, capture.Data).Extract<UdpPacket>();
                if (packet != null)
                    _receiver.ReceivePacket(packet.PayloadData);
            } catch (Exception exception) {
                Console.Error.WriteLine($"Failed to capture packet, exception: {exception}");
            }
        }

        public void Dispose() =>
            Stop();

        public Character LocalCharacter => Game.LocalCharacter.Instance;
    }
}
