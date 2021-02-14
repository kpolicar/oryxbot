using System;
using System.Threading;
using Albion.Network;
using OryxBot.Albion.Protocol;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Events;
using PacketDotNet;
using SharpPcap;

namespace OryxBot.Bot.Services
{
    public partial class NetworkAlbionDataProvider : AlbionDataProvider
    {
        private IPhotonReceiver receiver = null!;
        
        public event EventHandler<MoveEventArgs>? Move;

        public NetworkAlbionDataProvider() =>
            Run();
        

        private void Run() {
            var builder = ReceiverBuilder.Create();
    
            builder.AddRequestHandler(new MoveRequestHandler(this));
            // builder.AddEventHandler(new MoveEventHandler());
            // builder.AddEventHandler(new NewCharacterEventHandler());
            
            receiver = builder.Build();
    
            foreach (var device in CaptureDeviceList.Instance)
            {
                new Thread(() =>
                    {
                        Console.WriteLine($"Open... {device.Description}");
    
                        device.OnPacketArrival += PacketHandler;
                        device.Open(DeviceMode.Promiscuous, 1000);
                        device.StartCapture();
                    })
                    .Start();
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
    }
}
