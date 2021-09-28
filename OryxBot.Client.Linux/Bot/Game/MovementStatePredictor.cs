using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using OryxBot.Client.Linux.Bot.Services;
using OryxBot.Shared.Design;

namespace OryxBot.Client.Linux.Bot.Game
{
    public partial class LocalCharacter
    {
        private class MovementStatePredictor
        {
            private readonly LocalCharacter _character;
            private Position? _previousPosition;

            public double Speed {
                get;
                private set;
            } = 0d; // units per second
            private Stopwatch sw = new();
            private Task? characterPredictPositionJob;
            private CancellationTokenSource tokenSource = new ();

            public MovementStatePredictor(LocalCharacter character) {
                _character = character;
                _character.Move += OnCharacterMove;
                _character.ChangeCluster += OnCharacterChangeCluster;
            }

            private void OnCharacterChangeCluster(object? sender, EventArgs e) =>
                Speed = 0d;

            private void OnCharacterMove(object? sender, EventArgs e) {
                if (_previousPosition == null) {
                    _previousPosition = _character.Position;
                    sw.Start();
                    return;
                }
                
                Speed = 1d/sw.Elapsed.TotalSeconds * _character.DistanceFrom(_previousPosition.Value);
                _previousPosition = _character.Position;
                sw.Restart();

                if (!characterPredictPositionJob?.IsCompleted ?? false)
                    tokenSource.Cancel();
                _character.PredictedPosition = _character.Position;
                characterPredictPositionJob = Task.Run(CharacterPredictPositionJob, tokenSource.Token);
            }

            private async void CharacterPredictPositionJob() {
                try {
                    var waitedFor = 0;
                    
                    while (waitedFor < 2000) {
                        await Task.Delay(30);
                        waitedFor += 30;
                        if (Speed >= 30)
                            continue;
                        
                        var multiplier = sw.Elapsed.TotalSeconds * Speed;
                        _character.PredictedPosition = new Position(
                            (float)(multiplier * _character.Direction.X + _character.Position.X), 
                            (float)(multiplier * _character.Direction.Y + _character.Position.Y));
                    }
                } catch (OperationCanceledException) {
                }
            }
        }
    }
}
