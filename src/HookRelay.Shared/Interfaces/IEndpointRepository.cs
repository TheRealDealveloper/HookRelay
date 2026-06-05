using HookRelay.Shared.Models;

namespace HookRelay.Shared.Interfaces
{
    public interface IEndpointRepository
    {
        Task<Endpoint?> CreateAsync(Endpoint endpoint, CancellationToken ct = default);
        Task<Endpoint?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Endpoint?> GetByApiKeyAsync(string apiKey, CancellationToken ct = default);
        Task<IReadOnlyList<Endpoint>> GetAllAsync(CancellationToken ct = default);
        Task<Endpoint> UpdateAsync(Endpoint endpoint, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
