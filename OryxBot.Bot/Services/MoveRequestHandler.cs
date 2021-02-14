using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
using Albion.Network;
using OryxBot.Albion.Protocol;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Events;

namespace OryxBot.Bot.Services
{
    public partial class NetworkAlbionDataProvider
    {
        internal class MoveRequestHandler : RequestPacketHandler<MoveOperation>
        {
            private NetworkAlbionDataProvider DataProvider;

            public MoveRequestHandler(NetworkAlbionDataProvider networkAlbionDataProvider) :
                base(OperationCodes.Move) =>
                DataProvider = networkAlbionDataProvider;

            protected override Task OnActionAsync(MoveOperation operation) {
                DataProvider.Move?.Invoke(this, (MoveEventArgs) operation);
                // var waypoint = Path.currentTarget;
                // var position = new Point((int)operation.Position[0], (int)operation.Position[1]);
                //
                // if (Helpers.Math.Distance(position, waypoint) < 2) {
                //     Debug.WriteLine($"Waypoint reached {waypoint}");
                //     Path.currentTargetIndex += 1;
                //     if (Path.isComplete)
                //         Path.currentTargetIndex = 0;
                // }
                //
                // var direction = new Vector2(waypoint.X - position.X, waypoint.Y - position.Y);
                // direction = Vector2.Transform(direction, Matrix3x2.CreateRotation(-(float)Math.PI/4));
                // var dir = Vector2.Normalize(direction);
                // MoveMouseTo = (dir.X, -dir.Y);

                return Task.CompletedTask;
            }
        }
    }
}
