using Dapper;
using HookRelay.Shared.Interfaces;
using HookRelay.Shared.Models;
using Npgsql;

namespace HookRelay.Shared.Repositories
{
    public class PostgresDeliveryAttemptRepository : IDeliveryAttemptRepository
    {
        private readonly string _connectionString;
        public PostgresDeliveryAttemptRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        public Task<IReadOnlyList<DeliveryAttempt>> GetAllAttemptsAsync(Guid webhookEventId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<DeliveryAttempt?> GetLatestAttemptByWebhookEventIdAsync(Guid webhookEventId, CancellationToken ct = default)
        {
            const string sql = @"SELECT * FROM delivery_attempt WHERE webhook_id = @Id ORDER BY attempt_number DESC LIMIT 1";
            using var connection = new NpgsqlConnection(_connectionString);
            var result = await connection.QueryFirstOrDefaultAsync<DeliveryAttempt>(sql, new { Id = webhookEventId });
            return result;
        }

        public async Task<DeliveryAttempt?> SaveAsync(DeliveryAttempt deliveryAttempt, CancellationToken ct = default)
        {
            const string sql = @"INSERT INTO delivery_attempt (webhook_id, endpoint_id, attempt_number, status_code, response_body, error, duration_ms, delivery_status, attempted_at) 
                VALUES (@WebhookId, @EndpointId, @AttemptNumber, @StatusCode, @ResponseBody, @Error, @DurationMs, @DeliveryStatus, @AttemptedAt) RETURNING *";
            using var connection = new NpgsqlConnection(_connectionString);
            var result = await connection.QueryFirstOrDefaultAsync<DeliveryAttempt>(sql, deliveryAttempt);
            return result;
        }
    }
}
