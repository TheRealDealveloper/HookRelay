using Dapper;
using HookRelay.Dashboard.Components;
using HookRelay.Shared.Interfaces;
using HookRelay.Shared.Repositories;
using HookRelay.Shared.TypeHandlers;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.Configuration["IngestionApi:BaseUrl"]!)
});

var connectionString = builder.Configuration["ConnectionStrings:PostgreSQL"] ?? "";
builder.Services.AddSingleton<IEndpointRepository>(new PostgresEndpointRepository(connectionString));
builder.Services.AddSingleton<IWebhookRepository>(new PostgresWebhookRepository(connectionString));
builder.Services.AddSingleton<IDeliveryAttemptRepository>(new PostgresDeliveryAttemptRepository(connectionString));

SqlMapper.AddTypeHandler(new JsonDictionaryTypeHandler());
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
