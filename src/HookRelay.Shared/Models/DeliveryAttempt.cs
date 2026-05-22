using HookRelay.Shared.Enums;

namespace HookRelay.Shared.Models
{
    public class DeliveryAttempt
    {
        public Guid Id { get; set; }
        public Guid WebhookId { get; set; } 
        public Guid EndpointId {  get; set; } 
        public int AttemptNumber {  get; set; } = 0;
        public int? StatusCode { get; set; }
        public string? ResponseBody { get; set; }
        public string? Error { get; set; }
        public long DurationMs { get; set; }
        public DeliveryStatus DeliveryStatus { get; set; }
        public DateTime AttemptedAt { get; set; }
    }
}
