using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using OryxBot.Client.Linux.Bot.Services;
using OryxBot.Shared.Design;

namespace OryxBot.Client.Linux.Bot.Game
{
    public partial class LocalCharacter
    {
        public long MillisecondsSinceClusterChange => stateTracker.ClusterChangeWatch.ElapsedMilliseconds;
        
        private class MovementStateTracker
        {
            static MovementStateTracker() {
                AverageClusterChangeDuration = DefaultAverageClusterChangeDuration;
            }
        
            private const int DefaultAverageClusterChangeDuration = 10000;
            private static readonly int AverageClusterChangeDuration;
            
            private const float MinDistanceConsideredAsMove = 0.2f;
            private int IdleTimeout =>
                System.Math.Max(NetworkAlbionDataProvider.QueryNetworkInterval * 4, 1300);
            private int RecentlyChangeClusterDuration => AverageClusterChangeDuration*3;
            private readonly LocalCharacter _character;
            private Stopwatch sw = new();
            public Stopwatch ClusterChangeWatch { get; } = new();
            public Stopwatch IdleWatch { get; } = new();
            private Task movementTimeoutTask = Task.CompletedTask;
            private Task clusterChangeTimeoutTask = Task.CompletedTask;
            private Position? _previousPosition;

            public MovementStateTracker(LocalCharacter character) {
                _character = character;
                _character.Move += OnCharacterMove;
                _character.MovingChanged += OnMovingChanged;
                _character.ChangeCluster += OnCharacterChangeCluster;
            }

            private void OnMovingChanged(object? sender, EventArgs e) {
                if (_character.Moving) {
                    IdleWatch.Reset();
                } else {
                    IdleWatch.Restart();
                }
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
                
                ClusterChangeWatch.Restart();
                if (clusterChangeTimeoutTask.IsCompleted)
                    clusterChangeTimeoutTask = Task.Run(OnClusterTimeoutTaskTick);
            }

            private async Task OnTimeoutTaskTick() {
                while (sw.ElapsedMilliseconds < IdleTimeout) {
                    await Task.Delay(10);
                }
                _character.Moving = false;
            }

            private async Task OnClusterTimeoutTaskTick() {
                while (ClusterChangeWatch.ElapsedMilliseconds < RecentlyChangeClusterDuration) {
                    await Task.Delay(1000);
                }
                _character.RecentlyChangedCluster = false;
                Debug.WriteLine("recently changed cluster: false");
            }
        }
    }
}
