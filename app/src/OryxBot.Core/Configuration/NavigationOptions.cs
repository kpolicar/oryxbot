namespace OryxBot.Core.Configuration;

public class NavigationOptions
{
    public float ArrivalDistance { get; set; } = 2.0f;
    public TimeSpan IdleTimeout { get; set; } = TimeSpan.FromMilliseconds(1300);
    public TimeSpan UnstickBackupDuration { get; set; } = TimeSpan.FromMilliseconds(700);
    public TimeSpan UnstickCircleDuration { get; set; } = TimeSpan.FromMilliseconds(2500);
    public TimeSpan UnstickProbeDuration { get; set; } = TimeSpan.FromMilliseconds(500);
    public int UnstickMaxCircleFlips { get; set; } = 1;
    public float AssumedObstacleRadius { get; set; } = 3.0f;
    public float ObstacleRadiusGrowth { get; set; } = 2.0f;
    public int MaxStuckAttempts { get; set; } = 5;
    public TimeSpan StuckWindow { get; set; } = TimeSpan.FromSeconds(30);
    public float CorrectionEnterThreshold { get; set; } = 20.0f;
    public float CorrectionExitThreshold { get; set; } = 12.0f;
    public float LostThreshold { get; set; } = 30.0f;
    public TimeSpan LostTimeout { get; set; } = TimeSpan.FromSeconds(10);
    public int StalePacketsAfterClusterChange { get; set; } = 5;
    public TimeSpan ClusterLoadTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan DisconnectTimeout { get; set; } = TimeSpan.FromSeconds(15);
    public float IsometricRotation { get; set; } = -MathF.PI / 4f;
}
