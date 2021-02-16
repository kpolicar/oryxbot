using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;

namespace OryxBot.Bot
{
    public partial class TradeMissionRecord : Job, HasDependencies, IDisposable
    {
        private const float DistanceStep = 3;
        
        private TradeMissionRecordState state = new();
        private AlbionDataProvider dataProvider = null!;
        

        public void BindDependencies(ServiceContainer serviceContainer) {
            dataProvider = ((AlbionDataProvider) serviceContainer.GetService(typeof(AlbionDataProvider)))!;
            dataProvider.Move += RuntimeEventListener<MoveEventArgs>(OnCharacterMove);
        }

        private void OnCharacterMove(object? sender, MoveEventArgs e) {
            if (state.RecordedPositions.Count == 0) {
                RecordPosition(e.Position);
                return;
            }

            if (Helpers.Math.Distance(state.RecordedPositions.Last!.Value, e.Position) <= DistanceStep)
                return;
            
            RecordPosition(e.Position);
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
            if (state.RecordedPositions.Count <= 0)
                return;
            
            var fileName = GenerateFileName();
            using var fileStream = new StreamWriter($"recordings/{fileName}");

            lock (state) {
                foreach (var recordedPosition in state.RecordedPositions) {
                    fileStream.WriteLine($"{recordedPosition.X},{recordedPosition.Y}");
                }
            }
        }

        private string GenerateFileName() =>
            $"{DateTime.Now:yyyy-MM-dd_hh-mm-ss}.csv";

        private void RecordPosition(Position position) {
            state.RecordedPositions.AddLast(position);
            Debug.WriteLine($"Recorded Position {position}");
        }

        public void Dispose() {
            dataProvider.Move -= OnCharacterMove;
        }
    }
}
