using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using OryxBot.Bot.Attributes;
using OryxBot.Bot.Contracts;
using OryxBot.Bot.Game;
using OryxBot.Bot.Services;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;
using OryxBot.Shared.Extensions;
using ServiceContainer = OryxBot.Shared.Design.ServiceContainer;

namespace OryxBot.Bot
{
    public partial class TradeMissionRun : Job, HasDependencies
    {
        private const float MaxDistance = 4f;
        private const int MaxSkippableSteps = 4;
        private const int ClusterAvgLoadTime = 5000;
        private const int DelayBetweenNpcInterfaceActions = 2000;

        private event EventHandler? RouteFinished = null;

        private TradeMissionStep _step = new BankItems();
        
        private AlbionDataProvider dataProvider = null!;
        private static TradeMissionRouteProvider routeProvider = null!;
        private LinkedList<TradeMissionRecord.RecordableStep>? ActiveRoute;
        private LinkedList<TradeMissionRecord.RecordableStep> Route;
        private LinkedList<TradeMissionRecord.RecordableStep> RouteBack;
        private ITwoWayEnumerator<TradeMissionRecord.RecordableStep>? Step;
        private static InputActionFactory actions = null!;
        
        private Thread? runningThread;


        public TradeMissionRun(LinkedList<TradeMissionRecord.RecordableStep> steps, LinkedList<TradeMissionRecord.RecordableStep> stepsBack) {
            Route = steps;
            RouteBack = stepsBack;
        }
        
        public void BindDependencies(ServiceContainer serviceContainer) {
            dataProvider = serviceContainer.GetService<AlbionDataProvider>();
            routeProvider = serviceContainer.GetService<TradeMissionRouteProvider>();
            actions = (serviceContainer.GetService<ActionFactory>() as InputActionFactory)!;
        }

        public override void Start() {
            base.Start();

            runningThread = new Thread(EntryPoint);
            runningThread.Start();
        }

        public override void Stop() {
            base.Stop();
            Step?.Reset();
        }

        public void EntryPoint() {
            while (Running) {
                _step.Tick();
                Thread.Sleep(_step.Delay);
                
                if (_step.Finished)
                    ProgressToNextStep();
            }
        }

        private void ProgressToNextStep() {
            _step = _step switch {
                RunToBank => new BankItems(),
                BankItems => new RunToQuest(),
                RunToQuest => new TakeQuest(),
                TakeQuest => new RunRouteToDestination(Route),
                RunRouteToDestination => new ProgressQuest(),
                ProgressQuest => new RunRouteBack(RouteBack),
                RunRouteBack => new FinishQuest(),
                FinishQuest => new RunToBank(),
                _ => throw new ArgumentOutOfRangeException(nameof(_step))
            };
        }

        private Task RunRoute(LinkedList<TradeMissionRecord.RecordableStep> route, Func<object?, EventArgs, Task>? afterRoute = null) {
            State.Action = RUNNING_ROUTE;
            
            ActiveRoute = route;
            Step = ActiveRoute.GetTwoWayEnumerator();
            Step.MoveNext();
            
            funcToCallAfterRoute = afterRoute;
            RouteFinished += OnRunRouteFinished;

            return KeepTryingToMoveUntilValidMovement();
        }

        private void OnRunRouteFinished(object? o, EventArgs e) {
            funcToCallAfterRoute?.Invoke(o, e).ConfigureAwait(false);
            RouteFinished -= OnRunRouteFinished;
        }

        private Func<object?, EventArgs, Task>? funcToCallAfterRoute;
    }
}
