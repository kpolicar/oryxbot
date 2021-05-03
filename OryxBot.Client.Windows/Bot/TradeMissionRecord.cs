using System;
using System.Diagnostics;
using System.IO;
using OryxBot.Client.Windows.Bot.Contracts;
using OryxBot.Client.Windows.Bot.Game;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;
using OryxBot.Shared.Game;
using ServiceContainer = OryxBot.Shared.Design.ServiceContainer;

namespace OryxBot.Client.Windows.Bot
{
    public partial class TradeMissionRecord : Job, HasDependencies
    {
        public TradeMissionRecordState State {
            private set;
            get;
        } = new();
        private AlbionDataProvider dataProvider = null!;
        private Shared.Contracts.BotManager manager = null!;
        private TradeMissionRouteManager routeManager = null!;
        public override bool IsPaused => false;
        public bool HasStartedQuest => State.Status == BotStatus.RecordingRoute;
        public bool HasProgressedQuest => State.Status == BotStatus.RecordingRouteBack;
        public EventHandler<BotEventArgs>? StatusChanged;

        public TradeMissionRecord() {
            State.StatusChanged += (_, _) => StatusChanged?.Invoke(this, new BotEventArgs(this));
        }

        public override void Start() {
            base.Start();
            StatusChanged?.Invoke(this, new BotEventArgs(this));
        }


        public void BindDependencies(ServiceContainer serviceContainer) {
            dataProvider = serviceContainer.GetService<AlbionDataProvider>();
            manager = serviceContainer.GetService<Shared.Contracts.BotManager>();
            routeManager = serviceContainer.GetService<TradeMissionRouteManager>();
            LocalCharacter.Instance.Move += RuntimeEventListener(OnCharacterMove);
            LocalCharacter.Instance.ChangeCluster += RuntimeEventListener(OnChangeCluster);
            LocalCharacter.Instance.ProgressedQuest += RuntimeEventListener(OnProgressQuest);
        }

        private void OnProgressQuest(object? sender, EventArgs e) {
            Debug.WriteLine("progressed!");
            lock (State) {
                if (HasStartedQuest) {
                    State.RecordedSteps.AddLast(new ProgressQuestStep());
                    State.Status = BotStatus.RecordingRouteBack;
                } else if (HasProgressedQuest) {
                    Stop();
                } else {
                    State.Status = BotStatus.RecordingRoute;
                }
            }
        }

        private void OnCharacterMove(object? sender, EventArgs e) {
            lock (State) {
                if (!HasStartedQuest && !HasProgressedQuest)
                    return;
                State.RecordedSteps.AddLast(new MoveStep(LocalCharacter.Instance.Position));
            }
        }

        private void OnChangeCluster(object? sender, EventArgs e) {
            lock (State) {
                if (!HasStartedQuest && !HasProgressedQuest)
                    return;
                State.RecordedSteps.AddLast(new ChangeClusterStep(LocalCharacter.Instance.Cluster));
            }
        }

        public override void Stop() {
            if (!Running)
                return;
            base.Stop();
            SaveRecordingToDisk();
            Reset();
        }

        private void Reset() {
            lock (State) {
                State = new TradeMissionRecordState();
            }
        }

        private void SaveRecordingToDisk() {
            var configuration = manager.RecordingConfig;
            
            lock (State) {
                if (State.RecordedSteps.Count <= 0)
                    return;
            
                Directory.CreateDirectory("recordings");
                using var fileStream = routeManager.SaveRouteStream();

                fileStream.WriteLine(
                    "metadata,"+
                    $"origin:{Cities.Code(configuration.Origin)},"+
                    $"destination:{Regions.Code(configuration.Destination)},"+
                    $"name:{configuration.Name.Replace(",", ";")}");

                foreach (var recordedPosition in State.RecordedSteps) {
                    fileStream.WriteLine(recordedPosition.CsvFormat);
                }
            }
        }
    }
}
