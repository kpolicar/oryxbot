using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using OryxBot.Client.Linux.Bot.Contracts;
using OryxBot.Client.Linux.Bot.Events;
using OryxBot.Client.Linux.Bot.Exceptions;
using OryxBot.Client.Linux.Bot.Services;
using OryxBot.Client.Linux.Bot.Game;
using OryxBot.Shared;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Events;
using OryxBot.Shared.Extensions;
using OryxBot.Shared.Game;
using ServiceContainer = OryxBot.Shared.Design.ServiceContainer;

namespace OryxBot.Client.Linux.Bot
{
    public partial class TradeMissionRun : Job, HasDependencies
    {
        public event EventHandler? Finished;
        public event EventHandler? RunComplete;
        internal event EventHandler<TradeMissionEvent>? Progress;
        internal event EventHandler? Reset;
        internal event EventHandler<TradeMissionEvent>? Stuck;
        private const int DelayBetweenSteps = 1000;

        private TradeMissionStep _step;
        internal TradeMissionStep Step {
            get => _step;
            private set {
                if (_step is IDisposable disposable)
                    disposable.Dispose();
                _step = value;
                if (_step is RunRouteStep runStep) {
                    runStep.Stuck += OnRunStuck;
                }
                StatusChanged?.Invoke(this, new BotEventArgs(this));
                StepChanged?.Invoke(this, new BotEventArgs(this));
            }
        }

        private static TradeMissionRouteManager _routeManager = null!;
        public readonly TradeMissionRoute Route;
        private static InputActionFactory actions = null!;
        
        private Thread? runningThread;

        public bool _isPaused;
        public override bool IsPaused => _isPaused;
        public bool IsFirstRun = true;
        public int CurrentStepPercentComplete = 0;
        private City City;
        private City TradeCity;
        private RunConfiguration Config;
        public EventHandler<BotEventArgs>? StatusChanged;
        public EventHandler<BotEventArgs>? StepChanged;


        public TradeMissionRun(TradeMissionRoute route, RunConfiguration config)
        => (City, TradeCity, Route, Config) =
            ((City)route.Origin!, Npc.FactionEmissary.Allegiance[route.Destination!.Value], route, config);
        
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
            } else if (_isPaused) {
                Start();
            }
        }

        private void ResetRun() {
            if (IsFirstRun && Config.ResumeFromAlias != null) {
                Step = Config.HasProgresedQuest
                    ? new RunRouteBack(this, Route.RouteBack)
                    : new RunRouteToDestination(this, Route.RouteToNpc);
                
                (Step as RunRouteStep)!.SkipRouteToStep(
                    Config.ResumeFromAlias, LocalCharacter.Instance.Position);
            } else {
                Step = new RunToBank(this);
                //Step = new RunRouteBack(this, Route.RouteBack);
                Reset?.Invoke(this, EventArgs.Empty);
            }
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
            var previousStep = Step;
            Step = Step switch {
                RunToBank => new BankItems(Config.Contract),
                BankItems => new RunToQuest(this),
                RunToQuest => new TakeQuest(City, TradeCity, Config.Contract),
                TakeQuest => new RunRouteToDestination(this, Route.RouteToNpc),
                RunRouteToDestination => new ProgressQuest(),
                ProgressQuest => new RunRouteBack(this, Route.RouteBack),
                RunRouteBack => new FinishQuest(City),
                FinishQuest => new RunToBank(this),
                _ => throw new ArgumentOutOfRangeException(nameof(Step))
            };
            Progress?.Invoke(this, new TradeMissionEvent(Step));
            
            if (Step is RunToBank) {
                RunComplete?.Invoke(this, EventArgs.Empty);
            }
            if (Step is RunRouteStep runRoute) {
                runRoute.PercentCompleteChanged += OnRunRoutePercentCompleteChanged;
            }
            if (previousStep is RunRouteStep previousRunRoute) {
                previousRunRoute.PercentCompleteChanged -= OnRunRoutePercentCompleteChanged;
            }
        }

        private void OnRunRoutePercentCompleteChanged(object? sender, EventArgs e) {
            if (Step is RunRouteStep runRoute) {
                CurrentStepPercentComplete = runRoute.PercentComplete;
            }
        }

        private void OnRunStuck(object? sender, EventArgs e) {
            Pause();
            Stuck?.Invoke(this, new TradeMissionEvent(Step));
        }
    }
}
