using HookRelay.Shared.Interfaces;
using HookRelay.Shared.Models;
using System.Text;

namespace HookRelay.Delivery.Service
{
    public class WebhookDeliveryService : BackgroundService
    {
        private readonly IMessageBus _messageBus;
        private readonly IWebhookRepository _webhookRepo;
        private readonly IEndpointRepository _endpointRepo;
        private readonly HttpClient _httpClient;

        public WebhookDeliveryService(
            IMessageBus messageBus,
            IWebhookRepository webhookRepo,
            IEndpointRepository endpointRepo,
            HttpClient httpClient)
        {
            _messageBus = messageBus;
            _webhookRepo = webhookRepo;
            _endpointRepo = endpointRepo;
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

                var content = new StringContent(webhookEvent.Payload, Encoding.UTF8, "application/json");
                await _httpClient.PostAsync(endpoint.Url, content, ct);
            }, stoppingToken);
        }
    }
}
