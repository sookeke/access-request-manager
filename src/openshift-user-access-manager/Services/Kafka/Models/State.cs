namespace UserAccessManager.Services.Kafka.Models
{
    public class State
    {
        public const string Open = "Open";
        public const string Close = "Close";
        public const string Pending = "Pending";
        public const string InProgress = "In_Progress";
        public const string Approved = "Approved";
        public const string Queued = "Queued";
    }
}
