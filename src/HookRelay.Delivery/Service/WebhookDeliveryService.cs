using HookRelay.Shared.Enums;
using HookRelay.Shared.Interfaces;
using HookRelay.Shared.Models;
using System.Diagnostics;
using System.Text;

namespace HookRelay.Delivery.Service
{
    public class WebhookDeliveryService : BackgroundService
    {
        private const int MaxAttempts = 5;
        //private static readonly int[] RetryDelaysMs = [5_000, 30_000, 120_000, 600_000, 3_600_000];
        private static readonly int[] RetryDelaysMs = [3_000, 5_000, 8_000, 12_000, 15_000];

        private readonly IMessageBus _messageBus;
        private readonly IWebhookRepository _webhookRepo;
        private readonly IEndpointRepository _endpointRepo;
        private readonly IDeliveryAttemptRepository _deliveryAttemptRepo;
        private readonly HttpClient _httpClient;
        private readonly ILogger<WebhookDeliveryService> _logger;

        public WebhookDeliveryService(
            IMessageBus messageBus,
            IWebhookRepository webhookRepo,
            IEndpointRepository endpointRepo,
            IDeliveryAttemptRepository deliveryAttemptRepo,
            HttpClient httpClient,
            ILogger<WebhookDeliveryService> logger)
        {
            _messageBus = messageBus;
            _webhookRepo = webhookRepo;
            _endpointRepo = endpointRepo;
            _deliveryAttemptRepo = deliveryAttemptRepo;
            _httpClient = httpClient;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _messageBus.SubscribeAsync<WebhookMessage>(async (message, ct) =>
            {
                var webhookEvent = await _webhookRepo.GetByIdAsync(message.WebhookEventId, ct);
                if (webhookEvent == null)
                {
                    _logger.LogWarning("Webhook event {Id} not found, dropping message", message.WebhookEventId);
                    return;
                }

                var endpoint = await _endpointRepo.GetByIdAsync(webhookEvent.EndpointId, ct);
                if (endpoint == null)
                {
                    _logger.LogWarning("Endpoint {Id} not found for webhook {WebhookId}, dropping message", webhookEvent.EndpointId, webhookEvent.Id);
                    return;
                }

                var attemptNumber = message.AttemptNumber + 1;
                var delivery = new DeliveryAttempt
                {
                    WebhookId = webhookEvent.Id,
                    EndpointId = endpoint.Id,
                    AttemptNumber = attemptNumber,
                    DeliveryStatus = DeliveryStatus.Pending.ToString(),
                    AttemptedAt = DateTime.UtcNow
                };

                var saved = await _deliveryAttemptRepo.SaveAsync(delivery, ct);
                await DeliverAsync(saved, webhookEvent, endpoint, ct);
                await _deliveryAttemptRepo.UpdateAsync(saved, ct);

                if (saved.DeliveryStatus == DeliveryStatus.Success.ToString())
                    return;

                await HandleFailedDeliveryAsync(saved, message, attemptNumber, webhookEvent.Id, ct);
            }, stoppingToken);
        }

        private async Task DeliverAsync(DeliveryAttempt saved, WebhookEvent webhookEvent, Endpoint endpoint, CancellationToken ct)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var content = new StringContent(webhookEvent.Payload, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint.Url, content, ct);
                stopwatch.Stop();

                saved.StatusCode = (int)response.StatusCode;
                saved.ResponseBody = await response.Content.ReadAsStringAsync(ct);
                saved.DurationMs = stopwatch.ElapsedMilliseconds;
                saved.DeliveryStatus = response.IsSuccessStatusCode
                    ? DeliveryStatus.Success.ToString()
                    : DeliveryStatus.Failed.ToString();
            }
            catch (Exception e)
            {
                stopwatch.Stop();
                saved.DurationMs = stopwatch.ElapsedMilliseconds;
                saved.Error = e.Message;
                saved.DeliveryStatus = DeliveryStatus.Failed.ToString();
                _logger.LogError(e, "Delivery failed for webhook {WebhookId} attempt {Attempt}", saved.WebhookId, saved.AttemptNumber);
            }
        }

        private async Task HandleFailedDeliveryAsync(DeliveryAttempt saved, WebhookMessage message, int attemptNumber, Guid webhookId, CancellationToken ct)
        {
            if (attemptNumber < MaxAttempts)
            {
                message.AttemptNumber = attemptNumber;
                var delayMs = GetRetryDelayMs(attemptNumber);
                _logger.LogInformation("Scheduling retry {Attempt} for webhook {WebhookId} in {Delay}ms", attemptNumber + 1, webhookId, delayMs);
                await _messageBus.PublishToRetryAsync(message, delayMs, ct);
            }
            else
            {
                saved.DeliveryStatus = DeliveryStatus.DeadLettered.ToString();
                await _deliveryAttemptRepo.UpdateAsync(saved, ct);
                _logger.LogWarning("Webhook {WebhookId} dead-lettered after {MaxAttempts} attempts", webhookId, MaxAttempts);
                await _messageBus.PublishToDeadLetterAsync(message, ct);
            }
        }

        private static int GetRetryDelayMs(int attemptNumber) => RetryDelaysMs[attemptNumber - 1];
    }
}
