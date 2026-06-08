using HookRelay.Shared.Models;

namespace HookRelay.Shared.Interfaces
{
    public interface IDeliveryAttemptRepository
    {
        Task<DeliveryAttempt?> SaveAsync(DeliveryAttempt deliveryAttempt, CancellationToken ct = default);
        Task<IReadOnlyList<DeliveryAttempt>> GetAllAttemptsAsync(Guid webhookEventId, CancellationToken ct = default);
        Task<DeliveryAttempt?> GetLatestAttemptByWebhookEventIdAsync(Guid webhookEventId, CancellationToken ct = default);
        Task<Dictionary<Guid, DeliveryAttempt>> GetLatestAttemptsByWebhookIdsAsync(IEnumerable<Guid> webhookEventIds, CancellationToken ct = default);
    }
}
