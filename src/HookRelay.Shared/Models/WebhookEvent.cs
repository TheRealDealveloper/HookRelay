namespace HookRelay.Shared.Models
{
    public class WebhookEvent
    {
        public Guid Id { get; set; }
        public Guid EndpointId { get; set; } 
        public string HttpMethod {  get; set; } = string.Empty;
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public string Payload {  get; set; } = string.Empty;
        public string? QueryString {  get; set; }
        public string SourceIp {  get; set; } = string.Empty;
        public string ReceivedAt {  get; set; } = string.Empty;

    }
}
