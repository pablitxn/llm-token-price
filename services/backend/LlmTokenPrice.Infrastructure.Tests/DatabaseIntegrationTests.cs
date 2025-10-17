using FluentAssertions;
using LlmTokenPrice.Domain.Entities;
using LlmTokenPrice.Infrastructure.Tests.Factories;
using LlmTokenPrice.Infrastructure.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace LlmTokenPrice.Infrastructure.Tests;

/// <summary>
/// AC#9: Integration tests for PostgreSQL database connection, migrations, and entity creation.
/// Uses DatabaseFixture (TestContainers) for isolated testing with real PostgreSQL 16 instance.
/// </summary>
public class DatabaseIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public DatabaseIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    /// <summary>
    /// AC#9: Validates PostgreSQL connection establishment via TestContainer.
    /// </summary>
    [Fact]
    public async Task PostgreSQL_Connection_Should_Succeed()
    {
        // Arrange & Act
        await using var context = _fixture.CreateDbContext();

        // Assert
        var canConnect = await context.Database.CanConnectAsync();
        canConnect.Should().BeTrue("TestContainer PostgreSQL 16 should accept connections");
    }

    /// <summary>
    /// AC#9: Validates EF Core migrations execute successfully in TestContainer.
    /// </summary>
    [Fact]
    public async Task EFCore_Migrations_Should_Execute_Successfully()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();

        // Act
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();

        // Assert
        pendingMigrations.Should().BeEmpty("All migrations should be applied during fixture initialization");
        appliedMigrations.Should().NotBeEmpty("At least InitialSchema migration should be applied");
        appliedMigrations.Should().Contain(m => m.Contains("Initial"),
            "InitialSchema migration should be present");
    }

    /// <summary>
    /// AC#3, AC#9: Validates Respawn database cleanup completes in under 100ms.
    /// This is critical for meeting AC#13 integration test performance targets (<30s total).
    /// </summary>
    [Fact]
    public async Task Database_Cleanup_Should_Complete_In_Under_100ms()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync(); // Ensure clean state

        await using var context = _fixture.CreateDbContext();

        // Seed some data to make cleanup meaningful
        var benchmarks = SampleDataSeeder.CreateStandardBenchmarks();
        var models = SampleDataSeeder.CreateModels(5, benchmarks);

        await context.Benchmarks.AddRangeAsync(benchmarks);
        await context.Models.AddRangeAsync(models);
        await context.SaveChangesAsync();

        // Act
        var stopwatch = Stopwatch.StartNew();
        await _fixture.ResetDatabaseAsync();
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100,
            "Respawn cleanup must complete in <100ms to meet performance goals (AC#3)");

        // Verify cleanup worked
        await using var verifyContext = _fixture.CreateDbContext();
        var modelCount = await verifyContext.Models.CountAsync();
        var benchmarkCount = await verifyContext.Benchmarks.CountAsync();

        modelCount.Should().Be(0, "All models should be deleted after cleanup");
        benchmarkCount.Should().Be(0, "All benchmarks should be deleted after cleanup");
    }

    /// <summary>
    /// AC#9: Validates Model entity creation with relationships (Capability and BenchmarkScores).
    /// Tests the complete entity graph from the domain layer.
    /// </summary>
    [Fact]
    public async Task Model_Entity_Should_Be_Created_With_Valid_Relationships()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        await using var context = _fixture.CreateDbContext();

        var model = SampleDataSeeder.CreateModel("gpt-4", "OpenAI");
        model.Capability = SampleDataSeeder.CreateCapability(model.Id);

        var benchmarks = SampleDataSeeder.CreateStandardBenchmarks();
        await context.Benchmarks.AddRangeAsync(benchmarks);
        await context.SaveChangesAsync();

        model.BenchmarkScores = benchmarks
            .Take(3)
            .Select(b => SampleDataSeeder.CreateBenchmarkScore(model.Id, b.Id))
            .ToList();

        // Act
        await context.Models.AddAsync(model);
        await context.SaveChangesAsync();

        // Assert
        var savedModel = await context.Models
            .Include(m => m.Capability)
            .Include(m => m.BenchmarkScores)
            .FirstOrDefaultAsync(m => m.Id == model.Id);

        savedModel.Should().NotBeNull("Model should be saved to database");
        savedModel!.Name.Should().Be("gpt-4");
        savedModel.Provider.Should().Be("OpenAI");
        savedModel.Capability.Should().NotBeNull("Capability should be saved with Model");
        savedModel.BenchmarkScores.Should().HaveCount(3, "3 BenchmarkScores should be saved");
    }

    /// <summary>
    /// AC#9: Validates database schema has correct table structure with indexes and constraints.
    /// Queries PostgreSQL system tables to verify DDL was applied correctly.
    /// </summary>
    [Fact]
    public async Task Database_Schema_Should_Have_Correct_Table_Structure()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();

        // Act - Query PostgreSQL information schema
        var tables = await context.Database.SqlQueryRaw<string>(
            """
            SELECT table_name
            FROM information_schema.tables
            WHERE table_schema = 'public'
              AND table_type = 'BASE TABLE'
            ORDER BY table_name
            """
        ).ToListAsync();

        // Assert
        tables.Should().Contain("models", "models table should exist");
        tables.Should().Contain("model_capabilities", "model_capabilities table should exist");
        tables.Should().Contain("benchmarks", "benchmarks table should exist");
        tables.Should().Contain("model_benchmark_scores", "model_benchmark_scores table should exist");
    }

    /// <summary>
    /// AC#9: Validates indexes are created correctly for performance-critical queries.
    /// </summary>
    [Fact]
    public async Task Database_Schema_Should_Have_Required_Indexes()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();

        // Act - Query PostgreSQL indexes
        var indexes = await context.Database.SqlQueryRaw<string>(
            """
            SELECT indexname
            FROM pg_indexes
            WHERE schemaname = 'public'
            ORDER BY indexname
            """
        ).ToListAsync();

        // Assert - Verify critical indexes exist
        indexes.Should().Contain(i => i.Contains("model"), "Model table should have indexes");
        indexes.Should().Contain(i => i.Contains("benchmark"), "Benchmark tables should have indexes");
    }

    /// <summary>
    /// AC#9: Validates unique constraints are enforced at database level.
    /// Tests constraint violations throw appropriate exceptions.
    /// </summary>
    [Fact]
    public async Task Database_Should_Enforce_Unique_Constraints()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        await using var context = _fixture.CreateDbContext();

        var model1 = SampleDataSeeder.CreateModel("gpt-4", "OpenAI");
        await context.Models.AddAsync(model1);
        await context.SaveChangesAsync();

        // Act - Attempt to insert duplicate model name + provider
        var model2 = SampleDataSeeder.CreateModel("gpt-4", "OpenAI");
        await context.Models.AddAsync(model2);

        Func<Task> act = async () => await context.SaveChangesAsync();

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>(
            "Database should enforce unique constraint on (Name, Provider)");
    }

    /// <summary>
    /// AC#9: Validates cascade delete works correctly for Model → Capability → BenchmarkScores.
    /// </summary>
    [Fact]
    public async Task Deleting_Model_Should_Cascade_Delete_Relationships()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        await using var context = _fixture.CreateDbContext();

        var benchmarks = SampleDataSeeder.CreateStandardBenchmarks();
        await context.Benchmarks.AddRangeAsync(benchmarks);
        await context.SaveChangesAsync();

        var model = SampleDataSeeder.CreateModelWithRelationships(benchmarks, scoreCount: 3);
        await context.Models.AddAsync(model);
        await context.SaveChangesAsync();

        var capabilityId = model.Capability!.Id;
        var scoreIds = model.BenchmarkScores.Select(s => s.Id).ToList();

        // Act - Delete model
        context.Models.Remove(model);
        await context.SaveChangesAsync();

        // Assert - Capability and BenchmarkScores should be cascade deleted
        await using var verifyContext = _fixture.CreateDbContext();

        var deletedModel = await verifyContext.Models.FirstOrDefaultAsync(m => m.Id == model.Id);
        deletedModel.Should().BeNull("Model should be deleted");

        var deletedCapability = await verifyContext.Set<Capability>().FirstOrDefaultAsync(c => c.Id == capabilityId);
        deletedCapability.Should().BeNull("Capability should be cascade deleted");

        var remainingScores = await verifyContext.Set<BenchmarkScore>()
            .Where(s => scoreIds.Contains(s.Id))
            .CountAsync();
        remainingScores.Should().Be(0, "BenchmarkScores should be cascade deleted");
    }
}
