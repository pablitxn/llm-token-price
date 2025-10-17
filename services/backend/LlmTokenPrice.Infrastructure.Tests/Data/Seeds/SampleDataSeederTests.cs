using FluentAssertions;
using LlmTokenPrice.Infrastructure.Data;
using LlmTokenPrice.Infrastructure.Data.Seeds;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace LlmTokenPrice.Infrastructure.Tests.Data.Seeds;

/// <summary>
/// Unit tests for SampleDataSeeder to validate seeding logic and idempotency.
/// </summary>
public class SampleDataSeederTests
{
    /// <summary>
    /// Creates an in-memory DbContext for testing purposes.
    /// </summary>
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task SeedAsync_WithEmptyDatabase_SeedsCorrectCounts()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var logger = NullLogger.Instance;

        // Act
        await SampleDataSeeder.SeedAsync(context, logger);

        // Assert
        var modelCount = await context.Models.CountAsync();
        var benchmarkCount = await context.Benchmarks.CountAsync();
        var scoreCount = await context.BenchmarkScores.CountAsync();

        modelCount.Should().Be(10, "seeder should create exactly 10 models");
        benchmarkCount.Should().Be(5, "seeder should create exactly 5 benchmark definitions");
        scoreCount.Should().BeGreaterOrEqualTo(34, "seeder should create at least 34 benchmark scores (3+ per model)");
    }

    [Fact]
    public async Task SeedAsync_WithExistingData_SkipsSeeding()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var logger = NullLogger.Instance;

        // First seeding
        await SampleDataSeeder.SeedAsync(context, logger);
        var initialModelCount = await context.Models.CountAsync();
        var initialBenchmarkCount = await context.Benchmarks.CountAsync();
        var initialScoreCount = await context.BenchmarkScores.CountAsync();

        // Act - Second seeding attempt (should be skipped due to idempotency check)
        await SampleDataSeeder.SeedAsync(context, logger);

        // Assert
        var finalModelCount = await context.Models.CountAsync();
        var finalBenchmarkCount = await context.Benchmarks.CountAsync();
        var finalScoreCount = await context.BenchmarkScores.CountAsync();

        finalModelCount.Should().Be(initialModelCount, "idempotency check should prevent duplicate models");
        finalBenchmarkCount.Should().Be(initialBenchmarkCount, "idempotency check should prevent duplicate benchmarks");
        finalScoreCount.Should().Be(initialScoreCount, "idempotency check should prevent duplicate scores");
    }

    [Fact]
    public async Task SeedAsync_CreatesCorrectRelationships()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var logger = NullLogger.Instance;

        // Act
        await SampleDataSeeder.SeedAsync(context, logger);

        // Assert - Verify Model → Capability (1:1 relationship)
        var modelsWithCapabilities = await context.Models
            .Include(m => m.Capability)
            .ToListAsync();

        modelsWithCapabilities.Should().AllSatisfy(model =>
        {
            model.Capability.Should().NotBeNull("each model should have one capability entity (1:1 relationship)");
        });

        // Assert - Verify Model → BenchmarkScores (1:N relationship)
        var modelsWithScores = await context.Models
            .Include(m => m.BenchmarkScores)
            .ToListAsync();

        modelsWithScores.Should().AllSatisfy(model =>
        {
            model.BenchmarkScores.Should().NotBeEmpty("each model should have at least one benchmark score (1:N relationship)");
            model.BenchmarkScores.Count.Should().BeGreaterOrEqualTo(3, "each model should have at least 3 benchmark scores");
        });

        // Assert - Verify BenchmarkScore → Benchmark foreign key relationship
        var scores = await context.BenchmarkScores
            .Include(s => s.Benchmark)
            .ToListAsync();

        scores.Should().AllSatisfy(score =>
        {
            score.Benchmark.Should().NotBeNull("each benchmark score should reference a valid benchmark entity");
        });

        // Assert - Verify provider diversity (5 providers expected)
        var providers = modelsWithCapabilities.Select(m => m.Provider).Distinct().ToList();
        providers.Should().HaveCount(5, "seeded data should include 5 distinct providers");
        providers.Should().Contain(new[] { "OpenAI", "Anthropic", "Google", "Meta", "Mistral" });
    }
}
