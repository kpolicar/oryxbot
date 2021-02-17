using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        
        public event EventHandler<MoveEventArgs>? Move;
        
        public NetworkAlbionDataProvider() =>
            Run();

        class Packethan : PacketHandler<RequestPacket>
        {
            protected override Task OnHandleAsync(RequestPacket packet) {
                var packeta = (OperationCodes)packet.OperationCode;
                if (packeta == OperationCodes.Move)
                    return Task.CompletedTask;
                Debug.WriteLine("request: "+packeta);
                
                return Task.CompletedTask;
            }
        }

        class Packethana : PacketHandler<EventPacket>
        {
            protected override Task OnHandleAsync(EventPacket packet) {
                var packeta = (EventCodes)packet.EventCode;
                if (packeta == EventCodes.Move)
                    return Task.CompletedTask;
                Debug.WriteLine("event: "+packeta);
                
                return Task.CompletedTask;
            }
        }
        

        private void Run() {
            var builder = ReceiverBuilder.Create();
    
            builder.AddRequestHandler(new MoveRequestHandler(this));
            builder.AddHandler(new Packethan());
            builder.AddHandler(new Packethana());
            // builder.AddEventHandler(new MoveEventHandler());
            // builder.AddEventHandler(new NewCharacterEventHandler());
            
            receiver = builder.Build();
    
            foreach (var device in CaptureDeviceList.Instance) {
                var captureThread = new Thread(() => {
                    Console.WriteLine($"Open... {device.Description}");

                    device.OnPacketArrival += PacketHandler;
                    device.Open(DeviceMode.Promiscuous, 1000);
                    device.StartCapture();
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

        public void Dispose() {
            var stopTasks = CaptureDeviceList.Instance.Select(
                device => Task.Run(device.StopCapture));

            Task.WaitAll(stopTasks.ToArray());
        }
    }
}
