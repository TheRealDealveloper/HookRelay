using Dapper;
using HookRelay.Ingestion.Repositories;
using HookRelay.Ingestion.TypeHandlers;
using HookRelay.Shared.Interfaces;
using HookRelay.Shared.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
builder.Services.AddSingleton<IEndpointRepository>(new PostgresEndpointRepository(builder.Configuration["ConnectionStrings:PostgreSQL"] ?? ""));
builder.Services.AddSingleton<IWebhookRepository>(new PostgresWebhookRepository(builder.Configuration["ConnectionStrings:PostgreSQL"] ?? ""));
builder.Services.AddCors();

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
SqlMapper.AddTypeHandler(new JsonDictionaryTypeHandler());

var app = builder.Build();
app.UseCors(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

// Minimal API
app.MapPost("/hook/{apiKey}", async (string apiKey, HttpContext context, IEndpointRepository endpointRepo, IWebhookRepository webhookRepo)  => 
{
    var endpoint = await endpointRepo.GetByApiKeyAsync(apiKey);
    if (endpoint == null) return Results.NotFound();
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();

    var headers = new Dictionary<string, string>();
    foreach (var header in context.Request.Headers)
    {
        headers[header.Key] = header.Value.ToString();
    }

    var webhookEvent = new WebhookEvent
    {
        EndpointId = endpoint.Id,
        HttpMethod = context.Request.Method,
        Headers = headers,
        Payload = body,
        QueryString = context.Request.QueryString.Value,
        SourceIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown"
    };
    await webhookRepo.SaveAsync(webhookEvent);
    return Results.Ok();
});

app.Run();
