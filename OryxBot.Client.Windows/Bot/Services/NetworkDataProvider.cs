using System;
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

namespace OryxBot.Client.Windows.Bot.Services
{
    public partial class NetworkAlbionDataProvider : AlbionDataProvider, IDisposable, HasDependencies
    {
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
    
            foreach (var device in CaptureDeviceList.Instance) {
                var captureThread = new Thread(() => {
                    device.OnPacketArrival += PacketHandler;
                    device.Open(DeviceMode.Promiscuous, 200);
                    device.Filter = "ip and udp and (port 5056 or port 5055 or port 4535)";
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
        
        private void PacketHandler(object sender, CaptureEventArgs e)
        {
            try {
                UdpPacket packet = Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data).Extract<UdpPacket>();
                if (packet != null && (packet.SourcePort == 5056 || packet.DestinationPort == 5056)) {
                    _receiver.ReceivePacket(packet.PayloadData);
                }
            } catch (Exception exception) {
                Console.Error.WriteLine($"Failed to capture packet, exception: {exception}");
            }
        }

        public void Dispose() =>
            Stop();

        public Character LocalCharacter => Game.LocalCharacter.Instance;
    }
}
