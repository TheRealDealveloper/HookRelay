using HookRelay.Shared.Models;

namespace HookRelay.Shared.Interfaces
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class;
        Task SetAsync<T>(string key, T value, TimeSpan? expiry, CancellationToken ct = default) where T : class;
        Task RemoveAsync(string key, CancellationToken ct = default);
    }
}
