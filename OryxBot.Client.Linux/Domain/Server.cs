using System.IO;

namespace OryxBot.Client.Linux.Domain
{
    public class Server
    {
        public static readonly string BaseUrl = Program.Url;
        public static readonly string ApiUrl =
            $"{BaseUrl}/api/{Program.VersionEndpoint}/instance/"
            + File.ReadAllText("/etc/oryxbot.instance_endpoint").Replace("\n", "");
        public static readonly string AuthUrl = $"{BaseUrl}/oauth";
        public static readonly string BroadcastingAuthUrl = $"{ApiUrl}/broadcasting/auth";
        public static readonly bool WebsocketEncrypted = Program.WebsocketEncrypted;
        public static readonly string WebsocketHost = Program.WebsocketHost;
        public static readonly string PusherAppKey = Program.PusherAppKey;
    }
}
