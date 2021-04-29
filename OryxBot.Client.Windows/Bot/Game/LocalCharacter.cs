using System;
using System.Diagnostics;
using OryxBot.Shared.Contracts;
using OryxBot.Shared.Design;
using OryxBot.Shared.Game;

namespace OryxBot.Client.Windows.Bot.Game
{
    public partial class LocalCharacter : Character, HasDependencies
    {
        private static LocalCharacter _instance = new();
        public static LocalCharacter Instance => _instance;
        private ActionFactory actions = null!;
        public double Speed => movementPredictor.Speed;
        
        public void BindDependencies(ServiceContainer serviceContainer) {
            actions = serviceContainer.GetService<ActionFactory>();
        }

        public event EventHandler? Move;
        public event EventHandler? ChangeCluster;
        public event EventHandler? Interaction;
        public event EventHandler? MovingChanged;
        public event EventHandler? Died;
        public event EventHandler? ProgressedQuest;
        
        private MovementStateTracker stateTracker;
        private MovementStatePredictor movementPredictor;

        public void SendDieEvent() {
            Died?.Invoke(this, EventArgs.Empty);
        }

        private LocalCharacter() {
            stateTracker = new MovementStateTracker(this);
            movementPredictor = new MovementStatePredictor(this);
        }

        public double DistanceFrom(Position position) =>
            Helpers.Math.Distance(Position, position);

        private Position _position;
        public Position Position {
            get => _position;
            internal set {
                var old = _position;
                _position = value;
                if (!value.Equals(old))
                    Move?.Invoke(this, EventArgs.Empty);
            }
            
        }

        private string _cluster;
        public string Cluster {
            get => _cluster;
            internal set {
                _cluster = value;
                ChangeCluster?.Invoke(this, EventArgs.Empty);
            }
        }
        
        private bool _interacting;
        public bool Interacting {
            get => _interacting;
            internal set {
                var old = _interacting;
                _interacting = value;
                if (!value.Equals(old))
                    Interaction?.Invoke(this, EventArgs.Empty);
            }
        }
        
        private bool _moving;
        public bool Moving {
            get {
                return _moving;
            }
            internal set {
                var old = _moving;
                _moving = value;
                if (!value.Equals(old))
                    MovingChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool RecentlyChangedCluster {
            get; private set;
        }

        public void ProgressQuest() =>
            ProgressedQuest?.Invoke(this, EventArgs.Empty);
    }
}
