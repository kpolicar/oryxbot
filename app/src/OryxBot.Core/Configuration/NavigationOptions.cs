namespace OryxBot.Core.Configuration;

public class NavigationOptions
{
    public float ArrivalDistance { get; set; } = 2.0f;
    public TimeSpan IdleTimeout { get; set; } = TimeSpan.FromMilliseconds(1300);
    public float UnstickRotationNormal { get; set; } = -2f * MathF.PI / 3f;
    public float UnstickRotationPostCluster { get; set; } = -MathF.PI / 3f;
    public TimeSpan UnstickHoldDuration { get; set; } = TimeSpan.FromMilliseconds(1500);
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
