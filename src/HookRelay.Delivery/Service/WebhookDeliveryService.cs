using HookRelay.Shared.Enums;
using HookRelay.Shared.Interfaces;
using HookRelay.Shared.Models;
using System.Diagnostics;
using System.Text;

namespace HookRelay.Delivery.Service
{
    public class WebhookDeliveryService : BackgroundService
    {
        private readonly IMessageBus _messageBus;
        private readonly IWebhookRepository _webhookRepo;
        private readonly IEndpointRepository _endpointRepo;
        private readonly IDeliveryAttemptRepository _deliveryAttemptRepo;
        private readonly HttpClient _httpClient;

        public WebhookDeliveryService(
            IMessageBus messageBus,
            IWebhookRepository webhookRepo,
            IEndpointRepository endpointRepo,
            IDeliveryAttemptRepository deliveryAttemptRepo,
            HttpClient httpClient)
        {
            _messageBus = messageBus;
            _webhookRepo = webhookRepo;
            _endpointRepo = endpointRepo;
            _deliveryAttemptRepo = deliveryAttemptRepo;
            _httpClient = httpClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            await _messageBus.SubscribeAsync<WebhookMessage>(async (message, ct) =>
            {
                var webhookEvent = await _webhookRepo.GetByIdAsync(message.WebhookEventId, ct);
                if (webhookEvent == null) return;

                var endpoint = await _endpointRepo.GetByIdAsync(webhookEvent.EndpointId, ct);
                if (endpoint == null) return;

                var attempt = await _deliveryAttemptRepo.GetLatestAttemptByWebhookEventIdAsync(webhookEvent.Id);

                var content = new StringContent(webhookEvent.Payload, Encoding.UTF8, "application/json");
                var delivery = new DeliveryAttempt();
                delivery.WebhookId = webhookEvent.Id;
                delivery.EndpointId = endpoint.Id;
                delivery.AttemptNumber = attempt != null ? attempt.AttemptNumber + 1 : 1;
                delivery.DeliveryStatus = DeliveryStatus.Pending.ToString();

                Stopwatch stopwatch = new Stopwatch();
                try
                {
                    stopwatch.Start();
                    delivery.AttemptedAt = DateTime.UtcNow;
                    var response = await _httpClient.PostAsync(endpoint.Url, content, ct);
                    delivery.StatusCode = (int)response.StatusCode;
                    delivery.ResponseBody = await response.Content.ReadAsStringAsync(ct);
                    delivery.DeliveryStatus = response.IsSuccessStatusCode ? DeliveryStatus.Success.ToString() : DeliveryStatus.Failed.ToString();
                }
                catch (Exception e)
                {
                    delivery.Error = e.Message;
                    delivery.DeliveryStatus = DeliveryStatus.Failed.ToString();
                }
                finally
                {
                    stopwatch.Stop();
                }
                delivery.DurationMs = stopwatch.ElapsedMilliseconds;

                await _deliveryAttemptRepo.SaveAsync(delivery, ct);
            }, stoppingToken);
        }
    }
}
