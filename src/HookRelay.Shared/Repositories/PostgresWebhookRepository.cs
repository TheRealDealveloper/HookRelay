using Dapper;
using HookRelay.Shared.Interfaces;
using HookRelay.Shared.Models;
using Npgsql;

namespace HookRelay.Shared.Repositories
{
    public class PostgresWebhookRepository : IWebhookRepository
    {
        private readonly string _connectionString;
        public PostgresWebhookRepository(string connectionString) {
            _connectionString = connectionString;
        }

        public Task<int> CountByEndpointIdAsync(Guid id, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<WebhookEvent>> GetByEndpointIdAsync(Guid id, int page, int pageSize, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<WebhookEvent?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            const string sql = @"SELECT * from webhook_events WHERE id = @Id";
            using var connection = new NpgsqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<WebhookEvent>(sql, new { Id = id });
        }

        public async Task<WebhookEvent> SaveAsync(WebhookEvent webhookEvent, CancellationToken ct = default)
        {
            const string sql = @"INSERT INTO webhook_events
          (endpoint_id, http_method, headers, payload, query_string, source_ip)
          VALUES
          (@EndpointId, @HttpMethod, @Headers::jsonb, @Payload, @QueryString, @SourceIp)
          RETURNING *";

            using var connection = new NpgsqlConnection(_connectionString);
            return await connection.QuerySingleAsync<WebhookEvent>(sql, webhookEvent);
        }
    }
}
