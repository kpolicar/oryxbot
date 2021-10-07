namespace OryxBot.Client.Linux.Domain
{
    public class Server
    {
        public static readonly string BaseUrl = Program.Url;
        public static readonly string ApiUrl = $"{BaseUrl}/api/{Program.VersionEndpoint}";
        public static readonly string AuthUrl = $"{BaseUrl}/oauth";
        public static readonly string BroadcastingAuthUrl = $"{BaseUrl}/api/broadcasting/auth";
    }
}
