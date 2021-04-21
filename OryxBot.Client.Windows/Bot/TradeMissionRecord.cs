using System;
using System.IO;
using OryxBot.Client.Windows.Bot.Contracts;
using OryxBot.Client.Windows.Bot.Game;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Game;
using ServiceContainer = OryxBot.Shared.Design.ServiceContainer;

namespace OryxBot.Client.Windows.Bot
{
    public partial class TradeMissionRecord : Job, HasDependencies
    {
        private TradeMissionRecordState state = new();
        private AlbionDataProvider dataProvider = null!;
        private Shared.Contracts.BotManager manager = null!;
        private TradeMissionRouteManager routeManager = null!;
        private int justChangedCluster = 0;
        public override bool IsPaused => false;
        public bool HasStartedQuest = false;


        public void BindDependencies(ServiceContainer serviceContainer) {
            dataProvider = serviceContainer.GetService<AlbionDataProvider>();
            manager = serviceContainer.GetService<Shared.Contracts.BotManager>();
            routeManager = serviceContainer.GetService<TradeMissionRouteManager>();
            LocalCharacter.Instance.Move += RuntimeEventListener(OnCharacterMove);
            LocalCharacter.Instance.ChangeCluster += RuntimeEventListener(OnChangeCluster);
            LocalCharacter.Instance.ProgressedQuest += RuntimeEventListener(OnProgressQuest);
        }

        private void OnProgressQuest(object? sender, EventArgs e) {
            lock (state) {
                if (HasStartedQuest)
                    state.RecordedSteps.AddLast(new ProgressQuestStep());
                HasStartedQuest = true;
            }
        }

        private void OnCharacterMove(object? sender, EventArgs e) {
            lock (state) {
                if (!HasStartedQuest)
                    return;
                if (justChangedCluster > 0) {
                    justChangedCluster--;
                    return;
                }
                state.RecordedSteps.AddLast(new MoveStep(LocalCharacter.Instance.Position));
            }
        }

        private void OnChangeCluster(object? sender, EventArgs e) {
            lock (state) {
                if (!HasStartedQuest)
                    return;
                justChangedCluster = 5;
                state.RecordedSteps.AddLast(new ChangeClusterStep(LocalCharacter.Instance.Cluster));
            }
        }

        public override void Stop() {
            base.Stop();
            SaveRecordingToDisk();
            Reset();
        }

        private void Reset() {
            lock (state) {
                state = new TradeMissionRecordState();
                HasStartedQuest = false;
            }
        }

        private void SaveRecordingToDisk() {
            var configuration = manager.RecordingConfig;
            
            lock (state) {
                if (state.RecordedSteps.Count <= 0)
                    return;
            
                Directory.CreateDirectory("recordings");
                using var fileStream = routeManager.SaveRouteStream();

                fileStream.WriteLine(
                    "metadata,"+
                    $"origin:{Cities.Code(configuration.Origin)},"+
                    $"destination:{Regions.Code(configuration.Destination)},"+
                    $"name:{configuration.Name}");

                foreach (var recordedPosition in state.RecordedSteps) {
                    fileStream.WriteLine(recordedPosition.CsvFormat);
                }
            }
        }
    }
}
