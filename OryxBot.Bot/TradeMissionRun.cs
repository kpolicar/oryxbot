using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using OryxBot.Bot.Attributes;
using OryxBot.Bot.Contracts;
using OryxBot.Bot.Exceptions;
using OryxBot.Shared;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;
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
        private IEnumerator<TradeMissionRecord.RecordableStep>? Step;
        private ActionFactory actions = null!;
        private TradeMissionMovementTracker movementStateTracker;
        
        private MethodInfo[] OnMoveMethods = null!;
        private MethodInfo[] OnChangeClusterMethods = null!;
        private MethodInfo[] OnRegisterToObjectMethods = null!;


        public TradeMissionRun(LinkedList<TradeMissionRecord.RecordableStep> steps) {
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
            // todo InventoryMoveItem
            actions = serviceContainer.GetService<ActionFactory>();
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
        }

        public override void Start() {
            base.Start();
            movementStateTracker.Start();
            RunRouteFromQuestNpcToBank();
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
            }

            public TradeMissionAction Action {
                internal set;
                get;
            } = BANKING;

            public MoveEventArgs? LastKnownMove = null;
            public bool Moving = false;
        }

        private void OnRegisterToObject(object? sender, EventArgs e) {
            foreach (var method in OnRegisterToObjectMethods
                .Where(m => m.GetCustomAttributes(true).OfType<CallOnRegisterToObjectAttribute>().Any(attr => attr.RequiredState == State.Action))
            )
            {
                method.Invoke(this, new object?[] {e});
            }
        }

        private void OnCharacterMove(object? sender, MoveEventArgs e) {
            State.LastKnownMove = e;
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

        private void KeepTryingToMoveUntilValidMovement(TradeMissionRecord.MoveStep move) {
            while (!State.Moving) {
                Debug.WriteLine("still trying");
                actions.MoveTowards(default, move.Position);
                Thread.Sleep(1000);
            }
        }

        private void RunRoute(LinkedList<TradeMissionRecord.RecordableStep> route, Action<object?, EventArgs>? afterRoute = null) {
            State.Action = RUNNING_ROUTE;
            
            ActiveRoute = route;
            Step = ActiveRoute.GetEnumerator();
            Step.MoveNext();
            
            funcToCallAfterRoute = afterRoute;
            RouteFinished += OnRunRouteFinished;
            
            actions.MoveTowards(State.LastKnownMove?.Position ?? default, default);
        }

        private void OnRunRouteFinished(object? o, EventArgs e) {
            funcToCallAfterRoute?.Invoke(o, e);
            RouteFinished -= OnRunRouteFinished;
        }

        private Action<object?, EventArgs>? funcToCallAfterRoute;
    }
}
