using StackExchange.Redis;
using Testcontainers.Redis;

namespace LlmTokenPrice.Infrastructure.Tests.Fixtures;

/// <summary>
/// AC#2, AC#10: xUnit IAsyncLifetime fixture for Redis 7.2 TestContainer.
/// Provides shared Redis instance across test classes for cache integration testing.
/// </summary>
public class RedisFixture : IAsyncLifetime
{
    private RedisContainer? _redisContainer;

    /// <summary>
    /// The connection string to the test Redis instance.
    /// Available after InitializeAsync() completes.
    /// </summary>
    public string ConnectionString { get; private set; } = string.Empty;

    /// <summary>
    /// AC#10: Creates a new Redis ConnectionMultiplexer configured for the test container.
    /// </summary>
    /// <returns>A configured ConnectionMultiplexer</returns>
    public IConnectionMultiplexer CreateConnection()
    {
        if (string.IsNullOrEmpty(ConnectionString))
        {
            throw new InvalidOperationException(
                "RedisFixture not initialized. Ensure InitializeAsync() has been called.");
        }

        var options = ConfigurationOptions.Parse(ConnectionString);
        options.AbortOnConnectFail = false; // Graceful degradation as per production config
        options.ConnectTimeout = 5000;
        options.SyncTimeout = 5000;

        return ConnectionMultiplexer.Connect(options);
    }

    /// <summary>
    /// AC#2: Initializes the Redis 7.2 TestContainer (matches production version).
    /// Called once per test class that uses this fixture.
    /// </summary>
    public async Task InitializeAsync()
    {
        _redisContainer = new RedisBuilder()
            .WithImage("redis:7.2-alpine")
            .WithPortBinding(0, 6379) // Random host port to avoid conflicts
            .Build();

        await _redisContainer.StartAsync();

        ConnectionString = _redisContainer.GetConnectionString();
    }

    /// <summary>
    /// AC#2: Stops and disposes the Redis TestContainer.
    /// Called once after all tests in the test class complete.
    /// </summary>
    public async Task DisposeAsync()
    {
        if (_redisContainer != null)
        {
            await _redisContainer.DisposeAsync();
        }
    }
}
