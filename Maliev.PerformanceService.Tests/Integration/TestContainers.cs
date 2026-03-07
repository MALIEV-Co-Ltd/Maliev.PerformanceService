using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;

namespace Maliev.PerformanceService.Tests.Integration;

public static class TestContainers
{
    private static PostgreSqlContainer? _postgres;
    private static RedisContainer? _redis;
    private static RabbitMqContainer? _rabbitMq;
    private static readonly object _lock = new();
    private static bool _initialized;

    public static string PostgresConnectionString => _postgres?.GetConnectionString()
        ?? throw new InvalidOperationException("TestContainers not initialized. Call InitializeAsync() first.");

    public static string RedisConnectionString => _redis?.GetConnectionString()
        ?? throw new InvalidOperationException("TestContainers not initialized. Call InitializeAsync() first.");

    public static string RabbitMqConnectionString => _rabbitMq?.GetConnectionString()
        ?? throw new InvalidOperationException("TestContainers not initialized. Call InitializeAsync() first.");

    public static async Task InitializeAsync()
    {
        lock (_lock)
        {
            if (_initialized) return;

            _postgres = new PostgreSqlBuilder("postgres:18-alpine")
                .Build();

            _redis = new RedisBuilder("redis:8.4-alpine")
                .Build();

            _rabbitMq = new RabbitMqBuilder("rabbitmq:4.2-alpine")
                .Build();

            _initialized = true;
        }

        await Task.WhenAll(
            _postgres.StartAsync(),
            _redis.StartAsync(),
            _rabbitMq.StartAsync()
        );
    }

    public static async Task DisposeAsync()
    {
        if (_postgres is { } postgres)
        {
            await postgres.StopAsync();
        }

        if (_redis is { } redis)
        {
            await redis.StopAsync();
        }

        if (_rabbitMq is { } rabbitMq)
        {
            await rabbitMq.StopAsync();
        }

        lock (_lock)
        {
            _initialized = false;
            _postgres = null;
            _redis = null;
            _rabbitMq = null;
        }
    }
}
