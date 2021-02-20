using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using OryxBot.Bot.Exceptions;
using OryxBot.Shared;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;
using ServiceContainer = OryxBot.Shared.Design.ServiceContainer;

namespace OryxBot.Bot
{
    public partial class TradeMissionRun : Job, HasDependencies
    {
        private const float MaxDistance = 3f;
        private const int MaxSkippableSteps = 4;

        public TradeMissionRunState State {
            get;
            private set;
        } = new();
        private AlbionDataProvider dataProvider = null!;
        private LinkedList<TradeMissionRecord.RecordableStep> Route;
        private IEnumerator<TradeMissionRecord.RecordableStep> Step = null!;
        private ActionFactory actions = null!;


        public TradeMissionRun(LinkedList<TradeMissionRecord.RecordableStep> steps) =>
            Route = steps;
        
        public void BindDependencies(ServiceContainer serviceContainer) {
            dataProvider = serviceContainer.GetService<AlbionDataProvider>();
            dataProvider.Move += RuntimeEventListener<MoveEventArgs>(OnCharacterMove);
            dataProvider.ChangeCluster += RuntimeEventListener<ChangeClusterEventArgs>(OnChangeCluster);
            actions = serviceContainer.GetService<ActionFactory>();
        }

        public override void Start() {
            Step = Route.GetEnumerator();
            Step.MoveNext();
            
            base.Start();
        }

        public override void Stop() {
            base.Stop();
            State = new();
            Step.Reset();
        }

        private void OnCharacterMove(object? sender, MoveEventArgs e) {
            if (!(Step.Current is TradeMissionRecord.MoveStep target))
                return;

            Debug.WriteLine("yes");
            while (Step.Current is TradeMissionRecord.MoveStep move &&
                   Helpers.Math.Distance(move.Position, e.Position) <= MaxDistance)
            {
                Step.MoveNext();
                State.Executing = TradeMissionRunState.Action.MOVING;
                target = move;
            }
            actions.MoveTowards(e.Position, target.Position);
        }

        private void OnChangeCluster(object? sender, ChangeClusterEventArgs e) {
            for (var skips = 0 ;; skips++)
            {
                if (Step.Current is TradeMissionRecord.ChangeClusterStep)
                    break;
                if (skips >= MaxSkippableSteps)
                    throw new RouteException(Step.Current);

                Step.MoveNext();
            }

            var changeCluster = (Step.Current as TradeMissionRecord.ChangeClusterStep)!;
            if (changeCluster.Location != e.Location)
                throw new RouteException(Step.Current);
            Step.MoveNext();

            if (Step.Current is TradeMissionRecord.MoveStep move) {
                Task.Run(() => {
                    Thread.Sleep(5000);
                    actions.MoveTowards(default, move.Position);
                });
            }
        }
    }
}
