using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using OryxBot.Core.Abstractions;
using OryxBot.Core.Configuration;
using OryxBot.Core.Events;
using OryxBot.Core.Models;
using OryxBot.GameState;
using OryxBot.Input;
using OryxBot.Pilot;
using OryxBot.Pilot.States;
using OryxBot.Routes.Models;
using OryxBot.Routes.Traversal;
using OryxBot.Simulation.Obstacles;
using OryxBot.Simulation.Profiles;
using OryxBot.Simulation.Recording;

namespace OryxBot.Simulation.Fixtures;

public class SimulatorBuilder
{
    private Route? _route;
    private ISimulationProfile _profile = new PerfectProfile();
    private Position _startPosition = new(0, 0);
    private string _startCluster = "0208";
    private int _seed = 42;
    private int _maxTicks = 500;
    private string _testName = "UnnamedTest";
    private readonly List<Obstacle> _obstacles = [];
    private NavigationOptions? _navigationOptions;

    public SimulatorBuilder WithRoute(Route route) { _route = route; return this; }
    public SimulatorBuilder WithProfile(ISimulationProfile profile) { _profile = profile; return this; }
    public SimulatorBuilder WithProfile<T>() where T : ISimulationProfile, new() { _profile = new T(); return this; }
    public SimulatorBuilder StartingAt(Position position) { _startPosition = position; return this; }
    public SimulatorBuilder StartingInCluster(string cluster) { _startCluster = cluster; return this; }
    public SimulatorBuilder WithSeed(int seed) { _seed = seed; return this; }
    public SimulatorBuilder WithMaxTicks(int maxTicks) { _maxTicks = maxTicks; return this; }
    public SimulatorBuilder WithTestName(string name) { _testName = name; return this; }
    public SimulatorBuilder WithObstacle(Obstacle obstacle) { _obstacles.Add(obstacle); return this; }
    public SimulatorBuilder WithNavigationOptions(NavigationOptions options) { _navigationOptions = options; return this; }

    public SimulationHarness Build()
    {
        var route = _route ?? RouteFixtures.StraightLine();
        var navOptions = _navigationOptions ?? new NavigationOptions();
        var displayOptions = new DisplayOptions();

        var timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var navOptionsWrapper = Options.Create(navOptions);
        var displayOptionsWrapper = Options.Create(displayOptions);

        // GameState components
        var motionDetector = new MotionDetector(timeProvider, navOptionsWrapper);
        var positionPredictor = new PositionPredictor(timeProvider);
        var positionHistory = new PositionHistory(timeProvider);
        var tracker = new CharacterTracker(motionDetector, positionPredictor, positionHistory, timeProvider);

        // Pilot components
        var obstacleDetector = new ObstacleDetector(timeProvider, navOptionsWrapper);
        var obstacleMap = new ObstacleMap();

        var states = new INavigationState[]
        {
            new FollowingRouteState(navOptionsWrapper),
            new CorrectingCourseState(navOptionsWrapper),
            new UnstickingState(obstacleDetector, navOptionsWrapper, obstacleMap),
            new TransitioningState(navOptionsWrapper),
            new EvadingState(),
            new LostState(),
            new KilledState(),
            new DisconnectedState(),
        };

        var controller = new NavigationController(states);
        var cursor = new RouteCursor(route.Waypoints);

        // Recording
        var recorder = new SimulationRecorder(
            _testName, _profile.Name, route.Metadata.Name, _seed, _startCluster, route.Waypoints);
        recorder.AddObstacles(_obstacles);

        // Coordinate translator
        var coordinateTranslator = new CoordinateTranslator(displayOptionsWrapper, navOptionsWrapper);

        // Build simulator (needs to be created before input capture due to circular ref)
        var simulator = new GameSimulator(
            _profile, controller, cursor, tracker,
            null!, // GameController set below
            recorder, timeProvider, displayOptions,
            _obstacles, obstacleMap, _startPosition, _startCluster, _seed, _maxTicks);

        // Input capture → feeds back to simulator
        var inputCapture = new SimulatedInputCapture(simulator);

        // GameController → uses input capture
        var gameController = new GameController(inputCapture, tracker, coordinateTranslator);

        // Wire the game controller into the simulator via reflection or a setter
        simulator.SetGameController(gameController);

        return new SimulationHarness(simulator, controller, cursor, tracker,
            inputCapture, recorder, obstacleDetector, timeProvider);
    }
}

public record SimulationHarness(
    GameSimulator Simulator,
    NavigationController Controller,
    RouteCursor Cursor,
    CharacterTracker Tracker,
    SimulatedInputCapture InputCapture,
    SimulationRecorder Recorder,
    ObstacleDetector ObstacleDetector,
    FakeTimeProvider TimeProvider);
