using System;
using System.IO;
using OryxBot.Bot.Game;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using ServiceContainer = OryxBot.Shared.Design.ServiceContainer;

namespace OryxBot.Bot
{
    public partial class TradeMissionRecord : Job, HasDependencies
    {
        private TradeMissionRecordState state = new();
        private AlbionDataProvider dataProvider = null!;
        

        public void BindDependencies(ServiceContainer serviceContainer) {
            dataProvider = serviceContainer.GetService<AlbionDataProvider>();
            LocalCharacter.Instance.Move += RuntimeEventListener(OnCharacterMove);
            LocalCharacter.Instance.ChangeCluster += RuntimeEventListener(OnChangeCluster);
        }

        private void OnCharacterMove(object? sender, EventArgs e) {
            state.RecordedSteps.AddLast(new MoveStep(LocalCharacter.Instance.Position));
        }

        private void OnChangeCluster(object? sender, EventArgs e) {
            state.RecordedSteps.AddLast(new ChangeClusterStep(LocalCharacter.Instance.Cluster));
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
    }
}
