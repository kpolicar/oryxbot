using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using OryxBot.Client.Windows.Bot.Contracts;
using OryxBot.Client.Windows.Bot.Events;
using OryxBot.Client.Windows.Bot.Exceptions;
using OryxBot.Client.Windows.Bot.Services;
using OryxBot.Shared;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;
using OryxBot.Shared.Extensions;
using OryxBot.Shared.Game;
using ServiceContainer = OryxBot.Shared.Design.ServiceContainer;

namespace OryxBot.Client.Windows.Bot
{
    public partial class TradeMissionRun : Job, HasDependencies
    {
        public event EventHandler? Finished;
        public event EventHandler? RunComplete;
        internal event EventHandler<TradeMissionEvent>? Progress;
        internal event EventHandler? Reset;
        private const int DelayBetweenSteps = 1000;

        private TradeMissionStep _step;
        internal TradeMissionStep Step {
            get => _step;
            private set {
                _step = value;
                StatusChanged?.Invoke(this, new BotEventArgs(this));
            }
        }
        
        private static TradeMissionRouteManager _routeManager = null!;
        public readonly TradeMissionRoute Route;
        private static InputActionFactory actions = null!;
        
        private Thread? runningThread;

        public bool _isPaused;
        public override bool IsPaused => _isPaused;
        private City City;
        private City TradeCity;
        private RunConfiguration.ContractType Contract;
        public EventHandler<BotEventArgs>? StatusChanged;


        public TradeMissionRun(TradeMissionRoute route, RunConfiguration.ContractType contract)
        => (City, TradeCity, Route, Contract) =
            ((City)route.Origin!, Npc.FactionEmissary.Allegiance[route.Destination!.Value], route, contract);
        
        public void BindDependencies(ServiceContainer serviceContainer) {
            _routeManager = serviceContainer.GetService<TradeMissionRouteManager>();
            actions = (serviceContainer.GetService<ActionFactory>() as InputActionFactory)!;
        }

        public override void Start() {
            base.Start();
            if (!IsPaused)
                ResetRun();

            runningThread = new Thread(EntryPoint);
            runningThread.Start();
            StatusChanged?.Invoke(this, new BotEventArgs(this));
        }

        public override void Stop() {
            _isPaused = false;
            base.Stop();
        }

        public void Pause() {
            if (Running) {
                _isPaused = true;
                base.Stop();
            }
        }

        private void ResetRun() {
            Step = new RunRouteToDestination(this, Route.RouteToNpc);
            Reset?.Invoke(this, EventArgs.Empty);
        }

        public void EntryPoint() {
            while (Running) {
                try {
                    Step.Tick();
                } catch (OperationCanceledException) {
                } catch (CharacterDiedException) {
                    Debug.WriteLine(">>>>>>>>>>>>>>>>>>> CHARACTER HAS DIED. RESPAWNING!!");
                    actions.Respawn();
                    Thread.Sleep(2000);
                    Step = new RunToBank(this);
                }

                if (Step.Finished) {
                    Thread.Sleep(DelayBetweenSteps);
                    ProgressToNextStep();
                } else {
                    Thread.Sleep(Step.Delay);
                }
            }

            Finished?.Invoke(this, EventArgs.Empty);
        }

        private void ProgressToNextStep() {
            Step = Step switch {
                RunToBank => new BankItems(Contract),
                BankItems => new RunToQuest(this),
                RunToQuest => new TakeQuest(City, TradeCity, Contract),
                TakeQuest => new RunRouteToDestination(this, Route.RouteToNpc),
                RunRouteToDestination => new ProgressQuest(),
                ProgressQuest => new RunRouteBack(this, Route.RouteBack),
                RunRouteBack => new FinishQuest(City),
                FinishQuest => new RunToBank(this),
                _ => throw new ArgumentOutOfRangeException(nameof(Step))
            };
            Progress?.Invoke(this, new TradeMissionEvent(Step));
            Console.WriteLine("Progressed to next step: "+Step.GetType());
            
            if (Step is RunToBank) {
                RunComplete?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
