using HookRelay.Shared.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using System.Threading.Channels;

namespace HookRelay.Shared.Messaging
{
    public class RabbitMqMessageBus : IMessageBus, IAsyncDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private const string QueueDeliveryName = "webhook-delivery";
        private const string QueueDeadletterName = "webhook-deadletter";
        private const string QueueRetryName = "webhook-retry";

        public static async Task<RabbitMqMessageBus> CreateAsync(string connectionString)
        {
            var factory = new ConnectionFactory { Uri = new Uri(connectionString) };
            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();
            await channel.QueueDeclareAsync(
                queue: QueueDeliveryName,
                durable: true,
                exclusive: false,
                autoDelete: false);
            await channel.QueueDeclareAsync(
              queue: QueueDeadletterName,
              durable: true,
              exclusive: false,
              autoDelete: false);
            await channel.QueueDeclareAsync(
              queue: QueueRetryName,
              durable: true,
              exclusive: false,
              arguments: new Dictionary<string, object?> {
                { "x-dead-letter-exchange", "" },
                { "x-dead-letter-routing-key", QueueDeliveryName}
                },
              autoDelete: false);
            return new RabbitMqMessageBus(connection, channel);
        }

        private RabbitMqMessageBus(IConnection connection, IChannel channel)
        {
            _connection = connection;
            _channel = channel;
        }

        public async ValueTask DisposeAsync()
        {
            await _channel.CloseAsync();
            await _connection.CloseAsync();
        }

        public async Task PublishAsync<T>(T message, CancellationToken ct = default)
        {
            var body = JsonSerializer.SerializeToUtf8Bytes(message);
            await _channel.BasicPublishAsync(exchange: "", routingKey: QueueDeliveryName, body: body, cancellationToken: ct);
        }

        public async Task SubscribeAsync<T>(Func<T, CancellationToken, Task> handler, CancellationToken ct = default)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (sender, ea) => 
            {
                var message = JsonSerializer.Deserialize<T>(ea.Body.ToArray());
                await handler(message!, ct);
                await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: ct);
            };
            await _channel.BasicConsumeAsync(queue: QueueDeliveryName, autoAck: false, consumer: consumer, cancellationToken: ct);
        }

        public async Task PublishToRetryAsync<T>(T message, int delayMs, CancellationToken ct = default)
        {
            var body = JsonSerializer.SerializeToUtf8Bytes(message);
            await _channel.BasicPublishAsync(exchange: "", routingKey: QueueRetryName, false, new BasicProperties { Expiration = delayMs.ToString() }, body: body, cancellationToken: ct);
        }

        public async Task PublishToDeadLetterAsync<T>(T message, CancellationToken ct = default)
        {
            var body = JsonSerializer.SerializeToUtf8Bytes(message);
            await _channel.BasicPublishAsync(exchange: "", routingKey: QueueDeadletterName, body: body, cancellationToken: ct);
        }
    }
}
