using System;
using System.Diagnostics;
using System.Threading.Tasks;
using OryxBot.Shared.Design;

namespace OryxBot.Bot.Game
{
    public partial class LocalCharacter
    {
        private class MovementStateTracker
        {
            private const float MinDistanceConsideredAsMove = 0.2f;
            private const int StandStillDuration = 1000;
            private readonly LocalCharacter _character;
            private Stopwatch sw = new();
            private Task timeoutTask = Task.CompletedTask;
            private Position? _previousPosition;

            public MovementStateTracker(LocalCharacter character) {
                _character = character;
                _character.Move += OnCharacterMove;
            }
            
            private void OnCharacterMove(object? sender, EventArgs e) {
                _character.Moving =
                    _previousPosition == null ||
                    _character.DistanceFrom(_previousPosition.Value) >= MinDistanceConsideredAsMove;
                
                sw.Restart();
                if (timeoutTask.IsCompleted)
                    timeoutTask = Task.Run(OnTimeoutTaskTick);
                _previousPosition = _character.Position;
            }

            private async Task OnTimeoutTaskTick() {
                while (sw.ElapsedMilliseconds < StandStillDuration) {
                    await Task.Delay(10);
                }
                _character.Moving = false;
            }
        }
    }
}
