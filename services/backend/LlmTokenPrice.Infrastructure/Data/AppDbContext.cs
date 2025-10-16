using System.Reflection;
using LlmTokenPrice.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LlmTokenPrice.Infrastructure.Data;

/// <summary>
/// Application database context for LLM Token Pricing system.
/// Manages data persistence using Entity Framework Core with PostgreSQL + TimescaleDB.
/// </summary>
/// <remarks>
/// This DbContext follows Hexagonal Architecture principles:
/// - Lives in Infrastructure layer (adapter for data persistence)
/// - Domain entities are POCOs with no EF dependencies
/// - Entity configurations applied via Fluent API (Story 1.4)
/// </remarks>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the AppDbContext class.
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// DbSet for LLM model entities.
    /// </summary>
    public DbSet<Model> Models { get; set; } = null!;

    /// <summary>
    /// DbSet for model capability entities.
    /// </summary>
    public DbSet<Capability> Capabilities { get; set; } = null!;

    /// <summary>
    /// DbSet for benchmark definition entities.
    /// </summary>
    public DbSet<Benchmark> Benchmarks { get; set; } = null!;

    /// <summary>
    /// DbSet for model benchmark score entities.
    /// </summary>
    public DbSet<BenchmarkScore> BenchmarkScores { get; set; } = null!;

    /// <summary>
    /// Configures the model that was discovered by convention from entity types.
    /// Applies all IEntityTypeConfiguration implementations from this assembly.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Auto-discover and apply all entity configurations in Infrastructure assembly
        // Discovers: ModelConfiguration, CapabilityConfiguration, BenchmarkConfiguration, BenchmarkScoreConfiguration
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
