using System;
using System.Diagnostics;
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
            }
        }
    }
}
