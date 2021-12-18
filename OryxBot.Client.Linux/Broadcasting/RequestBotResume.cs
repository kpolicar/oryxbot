namespace OryxBot.Client.Linux.Broadcasting
{
    public class RequestBotResume
    {
        public int InstanceId { get; set; }
        public string City { get; set; }
        public string Alias { get; set; }
        public bool Progressed { get; set; }
        public int Hearts { get; set; }
    }
}
