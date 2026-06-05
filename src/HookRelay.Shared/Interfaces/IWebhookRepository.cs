using HookRelay.Shared.Models;

namespace HookRelay.Shared.Interfaces
{
    public interface IWebhookRepository
    {
        Task<WebhookEvent> SaveAsync(WebhookEvent webhookEvent, CancellationToken ct = default);
        Task<WebhookEvent?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<WebhookEvent>> GetByEndpointIdAsync(Guid id, CancellationToken ct = default);
        Task<int> CountByEndpointIdAsync(Guid id, CancellationToken ct = default);
    }
}
