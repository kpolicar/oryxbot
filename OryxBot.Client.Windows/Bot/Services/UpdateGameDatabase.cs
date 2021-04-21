using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Albion.Network;
using OryxBot.Albion.Protocol;
using OryxBot.Shared.Design;

namespace OryxBot.Client.Windows.Bot.Services
{
    public partial class NetworkAlbionDataProvider
    {
        private void BindEventRaiseHandlers(ReceiverBuilder builder) {
            builder.AddRequestHandler(new UpdateCharacterPosition());
            builder.AddRequestHandler(new UpdateCharacterCluster());
            builder.AddRequestHandler(new UpdateCharacterAddInteracting());
            builder.AddRequestHandler(new UpdateCharacterNotInteracting());
            builder.AddRequestHandler(new RaiseCharacterProgressedQuest());
            builder.AddEventHandler(new CharacterDied());
            //builder.AddHandler(new EventPacketLogger());
            //builder.AddHandler(new RequestPacketLogger());
            //builder.AddHandler(new ResponsePacketLogger());
        }

        private class EventPacketLogger : PacketHandler<EventPacket>
        {
            protected override Task OnHandleAsync(EventPacket packet) {
                if (packet.EventCode != 3 && packet.EventCode != 21)
                    Debug.WriteLine("event code: "+(EventCodes)packet.EventCode);
                return Task.CompletedTask;
            }
        }

        private class RequestPacketLogger : PacketHandler<RequestPacket>
        {
            protected override Task OnHandleAsync(RequestPacket packet) {
                Debug.WriteLine("request code: "+packet.OperationCode);
                throw new NotImplementedException();
            }
        }

        private class ResponsePacketLogger : PacketHandler<ResponsePacket>
        {
            protected override Task OnHandleAsync(ResponsePacket packet) {
                Debug.WriteLine("response code: "+packet.OperationCode);
                return Task.CompletedTask;
            }
        }
        
        private class CharacterDied : EventPacketHandler<DiedEvent>
        {
            public CharacterDied() :
                base((int) EventCodes.Died) {
            }
            
            protected override Task OnActionAsync(DiedEvent value) {
                Debug.WriteLine(">>>>>>>>>>>>>>>>>>> DEATH!!");
                
                return Task.Run(async () => {
                    for (int i = 0; i < 20; i++) {
                        await Task.Delay(100);
                        if (Game.LocalCharacter.Instance.Moving)
                            continue;
                        Debug.WriteLine(">>>>>>>>>>>>>>>>>>> DEATH EVENT MUST BE SENT!!");
                        Game.LocalCharacter.Instance.SendDieEvent();
                        return;
                    }
                });
            }
        }
        
        private class UpdateCharacterPosition : RequestPacketHandler<MoveOperation>
        {
            public UpdateCharacterPosition() :
                base((int) OperationCodes.Move) {
            }

            protected override Task OnActionAsync(MoveOperation value) {
                Game.LocalCharacter.Instance.Position = new Position(value.Position[0], value.Position[1]);
                return Task.CompletedTask;
            }
        }

        private class UpdateCharacterCluster : RequestPacketHandler<ChangeClusterOperation>
        {
            public UpdateCharacterCluster() :
                base((int) OperationCodes.ChangeCluster) {
            }

            protected override Task OnActionAsync(ChangeClusterOperation value) {
                Game.LocalCharacter.Instance.Cluster = value.Location;
                return Task.CompletedTask;
            }
        }

        private class UpdateCharacterAddInteracting : RequestPacketHandler<UnknownOperation>
        {
            public UpdateCharacterAddInteracting() :
                base((int) OperationCodes.RegisterToObject) {
            }
            
            protected override Task OnActionAsync(UnknownOperation value) {
                Game.LocalCharacter.Instance.Interacting = true;
                return Task.CompletedTask;
            }
        }

        private class UpdateCharacterNotInteracting : RequestPacketHandler<UnknownOperation>
        {
            public UpdateCharacterNotInteracting() :
                base((int) OperationCodes.UnRegisterFromObject) {
            }

            protected override Task OnActionAsync(UnknownOperation value) {
                Game.LocalCharacter.Instance.Interacting = false;
                return Task.CompletedTask;
            }
        }

        private class RaiseCharacterProgressedQuest : RequestPacketHandler<UnknownOperation>
        {
            public RaiseCharacterProgressedQuest() :
                base((int) OperationCodes.QuestGiverRequest) {
            }

            protected override Task OnActionAsync(UnknownOperation value) {
                Game.LocalCharacter.Instance.ProgressQuest();
                return Task.CompletedTask;
            }
        }
    }
}
