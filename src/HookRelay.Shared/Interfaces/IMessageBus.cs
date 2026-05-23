namespace HookRelay.Shared.Interfaces
{
    public interface IMessageBus
    {
        Task PublishAsync<T>(T message, CancellationToken ct = default);
        Task SubscribeAsync<T>(Func<T, CancellationToken, Task> handler, CancellationToken ct = default);
    }
}
