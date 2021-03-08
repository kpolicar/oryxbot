using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using OryxBot.Bot.Attributes;
using OryxBot.Bot.Contracts;
using OryxBot.Bot.Services;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;
using OryxBot.Shared.Extensions;
using static OryxBot.Bot.TradeMissionRun.TradeMissionRunState.TradeMissionAction;
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

        public TradeMissionRunState State {
            get;
            private set;
        } = new();
        private AlbionDataProvider dataProvider = null!;
        private TradeMissionRouteProvider routeProvider = null!;
        private LinkedList<TradeMissionRecord.RecordableStep>? ActiveRoute;
        private LinkedList<TradeMissionRecord.RecordableStep> Route;
        private LinkedList<TradeMissionRecord.RecordableStep> RouteBack;
        private ITwoWayEnumerator<TradeMissionRecord.RecordableStep>? Step;
        private InputActionFactory actions = null!;
        private TradeMissionMovementTracker movementStateTracker;
        
        private MethodInfo[] OnMoveMethods = null!;
        private MethodInfo[] OnChangeClusterMethods = null!;
        private MethodInfo[] OnRegisterToObjectMethods = null!;
        private MethodInfo[] OnUnregisterFromObjectMethods = null!;
        private Random rand = new();


        public TradeMissionRun(LinkedList<TradeMissionRecord.RecordableStep> steps, LinkedList<TradeMissionRecord.RecordableStep> stepsBack) {
            Route = steps;
            movementStateTracker = new TradeMissionMovementTracker(this);
        }
        
        public void BindDependencies(ServiceContainer serviceContainer) {
            dataProvider = serviceContainer.GetService<AlbionDataProvider>();
            routeProvider = serviceContainer.GetService<TradeMissionRouteProvider>();
            dataProvider.Move += (s, e) => 
                Debug.WriteLine(e.Position);
            dataProvider.Move += RuntimeEventListener<MoveEventArgs>(OnCharacterMove);
            dataProvider.ChangeCluster += RuntimeEventListener<ChangeClusterEventArgs>(OnChangeCluster);
            dataProvider.RegisterToObject += RuntimeEventListener(OnRegisterToObject);
            dataProvider.UnregisterFromObject += RuntimeEventListener(OnUnregisterFromObject);
            // todo InventoryMoveItem
            actions = (serviceContainer.GetService<ActionFactory>() as InputActionFactory)!;
            var hotkey = serviceContainer.GetService<Hotkey>();
            hotkey.F3 += (_, _) => Debug.WriteLine(State.LastKnownMove?.Position);
            movementStateTracker.BindDependencies(serviceContainer);

            
            OnMoveMethods = GetType()
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.GetCustomAttributes(typeof(CallOnMoveAttribute), true).Any())
                .ToArray();
            
            OnChangeClusterMethods = GetType()
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.GetCustomAttributes(typeof(CallOnChangeClusterAttribute), true).Any())
                .ToArray();
            
            OnRegisterToObjectMethods = GetType()
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.GetCustomAttributes(typeof(CallOnRegisterToObjectAttribute), true).Any())
                .ToArray();
            
            OnUnregisterFromObjectMethods = GetType()
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.GetCustomAttributes(typeof(CallOnUnregisterFromObjectAttribute), true).Any())
                .ToArray();
        }

        public override void Start() {
            base.Start();
            movementStateTracker.Start();
            Task.Run(RunRouteFromQuestNpcToBank).ConfigureAwait(false);
            //RunTradeMissionRoute();
        }

        public override void Stop() {
            base.Stop();
            State = new();
            Step?.Reset();
            movementStateTracker.Stop();
        }
        
        
        public class TradeMissionRunState
        {
            public enum TradeMissionAction
            {
                BANKING,
                TAKING_QUEST,
                RUNNING_ROUTE,
                PROGRESSING_QUEST,
            }

            public TradeMissionAction Action {
                internal set;
                get;
            } = BANKING;

            public MoveEventArgs? LastKnownMove = null;
            public bool Moving = false;
            public bool Interacting = false;
        }

        private void OnRegisterToObject(object? sender, EventArgs e) {
            State.Interacting = true;
            foreach (var method in OnRegisterToObjectMethods
                .Where(m => m.GetCustomAttributes(true).OfType<CallOnRegisterToObjectAttribute>().Any(attr => attr.RequiredState == State.Action))
            )
            {
                method.Invoke(this, new object?[] {e});
            }
        }

        private void OnUnregisterFromObject(object? sender, EventArgs e) {
            State.Interacting = false;
            foreach (var method in OnRegisterToObjectMethods
                .Where(m => m.GetCustomAttributes(true).OfType<CallOnUnregisterFromObjectAttribute>().Any(attr => attr.RequiredState == State.Action))
            )
            {
                method.Invoke(this, new object?[] {e});
            }
        }

        private void OnCharacterMove(object? sender, MoveEventArgs e) {
            State.LastKnownMove = e;
            Console.WriteLine("character moved: "+e.Position);
            foreach (var method in OnMoveMethods
                .Where(m => m.GetCustomAttributes(true).OfType<CallOnMoveAttribute>().Any(attr => attr.RequiredState == State.Action))
            )
            {
                method.Invoke(this, new object?[] {e});
            }
        }

        private void OnChangeCluster(object? sender, ChangeClusterEventArgs e) {
            foreach (var method in OnChangeClusterMethods
                .Where(m => m.GetCustomAttributes(true).OfType<CallOnChangeClusterAttribute>().Any(attr => attr.RequiredState == State.Action))
            )
            {
                method.Invoke(this, new object?[] {e});
            }
        }

        private Position CurrentOrigin() =>
            State.LastKnownMove?.Position ?? RandomPoint();
        private Position RandomPoint() =>
           new Position(rand.Next(100), rand.Next(100));

        private async Task KeepTryingToMoveUntilValidMovement(TradeMissionRecord.MoveStep? move = null) {
            do {
                Console.WriteLine("Attempting movement");
                actions.MoveTowards(CurrentOrigin(), move?.Position ?? RandomPoint(), true);
                
                await Task.Delay(1000).ConfigureAwait(false);
            } while (!State.Moving && Running);
        }

        private async Task KeepTryingToInteractUntilValidInteraction(Position target) {
            do {
                Console.WriteLine($"Attempting interaction with {target}, state: {State.Action}, current pos: "+CurrentOrigin());
                
                actions.StopAllActions();
                await Task.Delay(50).ConfigureAwait(false);
                    
                if (Helpers.Math.Distance(CurrentOrigin(), target) > 3f) {
                    actions.MoveTowards(CurrentOrigin(), target);
                    await Task.Delay(200).ConfigureAwait(false);
                    
                    actions.StopAllActions();
                    await Task.Delay(200).ConfigureAwait(false);
                    
                    continue;
                }
                actions.InteractWith(CurrentOrigin(), target);
                await Task.Delay(1000).ConfigureAwait(false);
            } while (!State.Interacting && Running);
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
