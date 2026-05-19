# Project 1: HookRelay — Webhook Relay & Inspector

### What it is

A platform for receiving, inspecting, replaying, and forwarding webhooks. Developers register endpoints, external services (Stripe, GitHub, etc.) send webhooks to HookRelay, and the platform logs every payload, streams them in real-time, and reliably forwards them to configured destinations with retry logic.

### Why it exists

Debugging webhooks in production is painful. You can't see what was sent, you can't replay a failed delivery, and when something breaks at 2am you're reading logs. Tools like Hookdeck and RequestBin exist in this space — this is a self-built alternative that demonstrates deep understanding of event-driven architecture.

### What it demonstrates

- Event-driven / microservice architecture
- Real-time streaming (SignalR)
- Message queuing with retry and dead letter handling (Azure Service Bus)
- Redis caching for dashboard stats
- Distributed system design (separate ingestion, delivery, and dashboard services)
- OpenTelemetry + Application Insights for observability
- Infrastructure as Code (Terraform/Bicep)
- CI/CD via GitHub Actions
- Containerized deployment to Azure Container Apps

### Tech stack

- ASP.NET Core Web API (C#)
- SignalR (real-time webhook streaming)
- Azure Service Bus (message queue)
- Redis (caching)
- PostgreSQL (webhook history, endpoint config)
- Docker
- Azure Container Apps
- Terraform or Bicep (IaC)
- GitHub Actions (CI/CD)
- OpenTelemetry + Azure Application Insights

### Architecture

Three services:

1. **Ingestion API** — receives incoming webhooks, validates, persists to database, publishes to Azure Service Bus
2. **Delivery Worker** — consumes from queue, forwards to configured destinations, handles retries with exponential backoff, moves to dead letter queue on permanent failure
3. **Dashboard API + Frontend** — displays live webhook stream via SignalR, browsable history with search/filter, replay button to re-enqueue a webhook, endpoint management, delivery metrics
