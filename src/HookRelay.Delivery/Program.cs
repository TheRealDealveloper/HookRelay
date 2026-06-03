using Dapper;
using HookRelay.Delivery.Service;
using HookRelay.Shared.Interfaces;
using HookRelay.Shared.Messaging;
using HookRelay.Shared.Repositories;
using HookRelay.Shared.TypeHandlers;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var connectionString = builder.Configuration["ConnectionStrings:PostgreSQL"] ?? "";
builder.Services.AddSingleton<IEndpointRepository>(new PostgresEndpointRepository(connectionString));
builder.Services.AddSingleton<IWebhookRepository>(new PostgresWebhookRepository(connectionString));

var messageBus = await RabbitMqMessageBus.CreateAsync(builder.Configuration["ConnectionStrings:RabbitMQ"] ?? "");
builder.Services.AddSingleton<IMessageBus>(messageBus);
SqlMapper.AddTypeHandler(new JsonDictionaryTypeHandler());

builder.Services.AddHttpClient<WebhookDeliveryService>();
builder.Services.AddHostedService<WebhookDeliveryService>();

var host = builder.Build();
host.Run();
