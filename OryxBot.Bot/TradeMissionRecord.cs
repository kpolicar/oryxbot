using System;
using System.IO;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;
using ServiceContainer = OryxBot.Shared.Design.ServiceContainer;

namespace OryxBot.Bot
{
    public partial class TradeMissionRecord : Job, HasDependencies, IDisposable
    {
        private const float DistanceStep = 3;
        
        private TradeMissionRecordState state = new();
        private AlbionDataProvider dataProvider = null!;
        

        public void BindDependencies(ServiceContainer serviceContainer) {
            dataProvider = serviceContainer.GetService<AlbionDataProvider>();
            dataProvider.Move += RuntimeEventListener<MoveEventArgs>(OnCharacterMove);
            dataProvider.ChangeCluster += RuntimeEventListener<ChangeClusterEventArgs>(OnChangeCluster);
        }

        private void OnCharacterMove(object? sender, MoveEventArgs e) {
            state.RecordedSteps.AddLast(MoveStep.From(e));
        }

        private void OnChangeCluster(object? sender, ChangeClusterEventArgs e) {
            state.RecordedSteps.AddLast(ChangeClusterStep.From(e));
        }

        public override void Stop() {
            base.Stop();
            SaveRecordingToDisk();
            Reset();
        }

        private void Reset() {
            state = new TradeMissionRecordState();
        }

        private void SaveRecordingToDisk() {
            if (state.RecordedSteps.Count <= 0)
                return;
            
            var fileName = GenerateFileName();
            using var fileStream = new StreamWriter($"recordings/{fileName}");

            lock (state) {
                foreach (var recordedPosition in state.RecordedSteps) {
                    fileStream.WriteLine(recordedPosition.CsvFormat);
                }
            }
        }

        private string GenerateFileName() =>
            $"{DateTime.Now:yyyy-MM-dd_hh-mm-ss}.csv";

        public void Dispose() {
            dataProvider.Move -= OnCharacterMove;
        }
    }
}
