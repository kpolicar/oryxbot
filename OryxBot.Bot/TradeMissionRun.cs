using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;
using ServiceContainer = OryxBot.Shared.Design.ServiceContainer;

namespace OryxBot.Bot
{
    public partial class TradeMissionRun : Job, HasDependencies
    {
        private const float MaxDistance = 0.5f;
        
        private TradeMissionRunState state = new();
        private AlbionDataProvider dataProvider = null!;
        private LinkedList<Position> Route;
        private IEnumerator<Position> Waypoint = null!;


        public TradeMissionRun(LinkedList<Position> route) =>
            Route = route;
        
        public void BindDependencies(ServiceContainer serviceContainer) {
            dataProvider = serviceContainer.GetService<AlbionDataProvider>();
            dataProvider.Move += RuntimeEventListener<MoveEventArgs>(OnCharacterMove);
        }

        public override void Start() {
            Waypoint = Route.GetEnumerator();
            base.Start();
        }

        private void OnCharacterMove(object? sender, MoveEventArgs e) {
            if (Helpers.Math.Distance(Waypoint.Current, e.Position) <= MaxDistance)
                Waypoint.MoveNext();
        }
    }
}
