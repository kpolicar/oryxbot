namespace OryxBot.Client.Linux.Broadcasting
{
    public class RequestBotRunningChanged
    {
        public int InstanceId { get; set; }
        public bool Running { get; set; }
        public string? City { get; set; }
        public int? Hearts { get; set; }
    }
}
