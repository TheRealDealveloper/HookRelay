namespace HookRelay.Shared.Models
{
    public class Endpoint
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
        public string ApiKey { get; set; } = "";
        public bool IsActive { get; set; } = true;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
