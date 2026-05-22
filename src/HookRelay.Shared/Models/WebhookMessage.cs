namespace HookRelay.Shared.Models
{
    public class WebhookMessage
    {
        public Guid WebhookEventId { get; set; }
        public int AttemptNumber { get; set; } = 0; 
        public DateTime EnqueuedAt { get; set; }
    }
}
