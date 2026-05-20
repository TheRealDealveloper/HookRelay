# HookRelay — Webhook Relay & Inspector

### What it is

A platform for receiving, inspecting, replaying, and forwarding webhooks. Developers register endpoints, external services (Stripe, GitHub, etc.) send webhooks to HookRelay, and the platform logs every payload, streams them in real time, and reliably forwards them to configured destinations with retry logic.

### Why it exists

Debugging webhooks in production is painful. You cannot see what was sent, you cannot replay a failed delivery, and when something breaks at 2am you are reading logs. Tools like Hookdeck and RequestBin exist in this space. This is a self-built alternative that demonstrates deep understanding of event-driven architecture.

### What it demonstrates

- Event-driven microservice architecture
- Real-time streaming (SignalR)
- Message queuing with retry and dead letter handling (Azure Service Bus)
- Redis caching for dashboard stats
- Distributed system design (separate ingestion, delivery, and dashboard services)
- OpenTelemetry + Application Insights for observability
- Infrastructure as Code (Terraform)
- CI/CD via GitHub Actions
- Containerized deployment to Azure Container Apps

### Tech stack

- ASP.NET Core Web API (C#)
- Blazor Server (dashboard frontend)
- SignalR (real-time webhook streaming)
- Azure Service Bus (message queue)
- PostgreSQL (webhook history, endpoint config)
- Redis (caching)
- Docker
- Azure Container Apps
- Terraform (IaC)
- GitHub Actions (CI/CD)
- OpenTelemetry + Azure Application Insights

### Architecture

Three services:

1. **Ingestion API** — receives incoming webhooks, validates, persists to database, publishes to Azure Service Bus
2. **Delivery Worker** — consumes from queue, forwards to configured destinations, handles retries with exponential backoff, moves to dead letter queue on permanent failure
3. **Dashboard** — Blazor Server app with live webhook stream via SignalR, browsable history with search and filter, replay button to re-enqueue a webhook, endpoint management, delivery metrics

### Design philosophy

The system is built for loose coupling throughout. Every infrastructure dependency (database, message bus, cache) is abstracted behind an interface so that the provider can be swapped by changing configuration alone, with no code changes required. This makes it straightforward to run free-tier providers during development and production-grade Azure services when deployed.
