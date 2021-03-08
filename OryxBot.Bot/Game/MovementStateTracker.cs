using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace OryxBot.Bot.Game
{
    public partial class LocalCharacter
    {
        private class MovementStateTracker
        {
            private const int StandStillDuration = 1000;
            private readonly LocalCharacter _character;
            private Stopwatch sw = new();
            private Task timeoutTask = Task.CompletedTask;

            public MovementStateTracker(LocalCharacter character) {
                _character = character;
                _character.Move += OnCharacterMove;
            }
            
            private void OnCharacterMove(object? sender, EventArgs e) {
                _character.Moving = true;
                sw.Restart();
                if (timeoutTask.IsCompleted)
                    timeoutTask = Task.Run(OnTimeoutTaskTick);
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
