using LlmTokenPrice.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace LlmTokenPrice.Infrastructure.Tests.Fixtures;

/// <summary>
/// AC#7: xUnit IAsyncLifetime fixture that provides shared test database instance across test classes.
/// Uses TestContainers for isolated PostgreSQL 16 instances and Respawn for fast cleanup (<100ms target).
/// </summary>
public class DatabaseFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _postgresContainer;
    private Respawner? _respawner;

    /// <summary>
    /// The connection string to the test database.
    /// Available after InitializeAsync() completes.
    /// </summary>
    public string ConnectionString { get; private set; } = string.Empty;

    /// <summary>
    /// AC#7: Creates a new DbContext instance configured for the test database.
    /// Each test should create its own context instance for proper isolation.
    /// </summary>
    /// <returns>A configured AppDbContext pointing to the TestContainer database</returns>
    public AppDbContext CreateDbContext()
    {
        if (string.IsNullOrEmpty(ConnectionString))
        {
            throw new InvalidOperationException(
                "DatabaseFixture not initialized. Ensure InitializeAsync() has been called.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder
            .UseNpgsql(ConnectionString)
            .UseLoggerFactory(LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Warning)));

        return new AppDbContext(optionsBuilder.Options);
    }

    /// <summary>
    /// AC#2, AC#7: Initializes the PostgreSQL 16 TestContainer and runs EF Core migrations.
    /// Called once per test class that uses this fixture (xUnit IClassFixture pattern).
    /// </summary>
    public async Task InitializeAsync()
    {
        // AC#2: Start PostgreSQL 16 TestContainer (matches production version)
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("llm_token_price_test")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .WithPortBinding(0, 5432) // Random host port to avoid conflicts
            .Build();

        await _postgresContainer.StartAsync();

        ConnectionString = _postgresContainer.GetConnectionString();

        // AC#7: Run EF Core migrations to create schema
        await using var context = CreateDbContext();
        await context.Database.MigrateAsync();

        // AC#3: Initialize Respawn for fast database cleanup (<100ms target)
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = ["__EFMigrationsHistory"] // Preserve migrations table
        });
    }

    /// <summary>
    /// AC#3: Resets the database to a clean state by deleting all data (except migrations).
    /// Target: <100ms cleanup time to meet AC#13 integration test performance goals.
    /// Call this method at the start of each test to ensure isolation.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        if (_respawner == null)
        {
            throw new InvalidOperationException("Respawner not initialized. Ensure InitializeAsync() was called.");
        }

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    /// <summary>
    /// AC#2: Stops and disposes the PostgreSQL TestContainer.
    /// Called once after all tests in the test class complete.
    /// </summary>
    public async Task DisposeAsync()
    {
        if (_postgresContainer != null)
        {
            await _postgresContainer.DisposeAsync();
        }
    }
}
