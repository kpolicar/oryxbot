using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Albion.Network;
using OryxBot.Albion.Protocol;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Events;
using PacketDotNet;
using SharpPcap;

namespace OryxBot.Bot.Services
{
    public partial class NetworkAlbionDataProvider : AlbionDataProvider, IDisposable
    {
        private List<Thread> captureThreads = new();
        private IPhotonReceiver receiver = null!;
        private event EventHandler? Disposing;
        
        public event EventHandler<MoveEventArgs>? Move;

        public NetworkAlbionDataProvider() =>
            Run();
        

        private void Run() {
            var builder = ReceiverBuilder.Create();
    
            builder.AddRequestHandler(new MoveRequestHandler(this));
            // builder.AddEventHandler(new MoveEventHandler());
            // builder.AddEventHandler(new NewCharacterEventHandler());
            
            receiver = builder.Build();
    
            foreach (var device in CaptureDeviceList.Instance) {
                var captureThread = new Thread(() => {
                    Console.WriteLine($"Open... {device.Description}");

                    device.OnPacketArrival += PacketHandler;
                    device.Open(DeviceMode.Promiscuous, 1000);
                    device.StartCapture();
                    
                    Disposing += (_, _) => device.StopCapture();
                });
                captureThreads.Add(captureThread);
                captureThread.Start();
            }
        }
        
        private void PacketHandler(object sender, CaptureEventArgs e)
        {
            UdpPacket packet = Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data).Extract<UdpPacket>();
            if (packet != null && (packet.SourcePort == 5056 || packet.DestinationPort == 5056))
            {
                receiver.ReceivePacket(packet.PayloadData);
            }
        }

        public void Dispose() =>
            Disposing?.Invoke(this, EventArgs.Empty);
    }
}
