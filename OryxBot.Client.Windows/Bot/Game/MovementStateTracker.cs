using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using OryxBot.Client.Windows.Bot.Services;
using OryxBot.Shared.Design;

namespace OryxBot.Client.Windows.Bot.Game
{
    public partial class LocalCharacter
    {
        private class MovementStateTracker
        {
            static MovementStateTracker() {
                var successfulParse =
                    int.TryParse(ConfigurationManager.AppSettings.Get("averageTimeToLoadCluster")!, out AverageClusterChangeDuration);
                if (!successfulParse)
                    AverageClusterChangeDuration = DefaultAverageClusterChangeDuration;
            }
        
            private const int DefaultAverageClusterChangeDuration = 10000;
            private static readonly int AverageClusterChangeDuration;
            
            private const float MinDistanceConsideredAsMove = 0.2f;
            private int StandStillDuration =>
                System.Math.Max(NetworkAlbionDataProvider.QueryNetworkInterval * 4, 1300);
            private int RecentlyChangeClusterDuration => AverageClusterChangeDuration*2;
            private readonly LocalCharacter _character;
            private Stopwatch sw = new();
            private Stopwatch sw_cluster = new();
            private Task movementTimeoutTask = Task.CompletedTask;
            private Task clusterChangeTimeoutTask = Task.CompletedTask;
            private Position? _previousPosition;

            public MovementStateTracker(LocalCharacter character) {
                _character = character;
                _character.Move += OnCharacterMove;
                _character.ChangeCluster += OnCharacterChangeCluster;
            }

            private void OnCharacterMove(object? sender, EventArgs e) {
                _character.Moving =
                    _previousPosition == null ||
                    _character.DistanceFrom(_previousPosition.Value) >= MinDistanceConsideredAsMove;
                
                sw.Restart();
                if (movementTimeoutTask.IsCompleted)
                    movementTimeoutTask = Task.Run(OnTimeoutTaskTick);
                _previousPosition = _character.Position;
            }

            private void OnCharacterChangeCluster(object? sender, EventArgs e) {
                _character.RecentlyChangedCluster = true;
                Debug.WriteLine("recently changed cluster: true");
                
                sw_cluster.Restart();
                if (clusterChangeTimeoutTask.IsCompleted)
                    clusterChangeTimeoutTask = Task.Run(OnClusterTimeoutTaskTick);
            }

            private async Task OnTimeoutTaskTick() {
                while (sw.ElapsedMilliseconds < StandStillDuration) {
                    await Task.Delay(10);
                }
                _character.Moving = false;
            }

            private async Task OnClusterTimeoutTaskTick() {
                while (sw_cluster.ElapsedMilliseconds < RecentlyChangeClusterDuration) {
                    await Task.Delay(1000);
                }
                _character.RecentlyChangedCluster = false;
                Debug.WriteLine("recently changed cluster: false");
            }
        }
    }
}
