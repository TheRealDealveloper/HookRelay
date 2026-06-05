using Dapper;
using HookRelay.Shared.Interfaces;
using HookRelay.Shared.Models;
using Npgsql;
using Endpoint = HookRelay.Shared.Models.Endpoint;

namespace HookRelay.Shared.Repositories
{
    public class PostgresEndpointRepository : IEndpointRepository
    {
        private readonly string _connectionString;
        public PostgresEndpointRepository(string connectionString)  
        {
            _connectionString = connectionString;
        }

        public async Task<Endpoint?> CreateAsync(Endpoint endpoint, CancellationToken ct = default)
        {
            const string sql = @"INSERT INTO endpoints (name, url, api_key, is_active) VALUES (@Name, @Url, @ApiKey, @IsActive) RETURNING *";
            using var connection = new NpgsqlConnection(_connectionString);
            var result = await connection.QueryFirstOrDefaultAsync<Endpoint>(sql, endpoint);
            return result;
        }

        public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyList<Endpoint>> GetAllAsync(CancellationToken ct = default)
        {
            const string sql = @"SELECT * from endpoints";
            using var connection = new NpgsqlConnection(_connectionString);
            var result = await connection.QueryAsync<Endpoint>(sql);
            return result.ToList();
        }

        public async Task<Endpoint?> GetByApiKeyAsync(string apiKey, CancellationToken ct = default)
        {
            Endpoint? result = null;
            const string sql = @"SELECT * FROM endpoints WHERE api_key = @ApiKey";

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            result = await connection.QueryFirstOrDefaultAsync<Endpoint?>(sql, new { ApiKey = apiKey });

            return result;
        }

        public async Task<Endpoint?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            const string sql = @"SELECT * from endpoints WHERE id = @Id";
            using var connection = new NpgsqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Endpoint>(sql, new { Id = id });
        }

        public Task<Endpoint> UpdateAsync(Shared.Models.Endpoint endpoint, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
