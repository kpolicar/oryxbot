using System;
using System.Diagnostics;
using System.Numerics;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;

namespace OryxBot.Bot.Services
{
    public class InputActionFactory : ActionFactory, HasDependencies
    {
        private Input input = null!;
        private AlbionDataProvider dataProvider = null!;
        private Position currentPosition;


        public void BindDependencies(ServiceContainer serviceContainer) {
            input = serviceContainer.GetService<Input>();
            dataProvider = serviceContainer.GetService<AlbionDataProvider>();
            dataProvider.Move += (_, e) => currentPosition = e.Position;
        }

        public void MoveTowards(Position origin, Position target) {
            var direction = new Vector2(target.X - origin.X, target.Y - origin.Y);
            direction = Vector2.Transform(direction, Matrix3x2.CreateRotation(-(float)Math.PI/4));
            direction = Vector2.Normalize(direction);

            input.MoveCursorRelativeToCenter(direction);
        }
    }
}
