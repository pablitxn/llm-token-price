using LlmTokenPrice.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LlmTokenPrice.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for the BenchmarkScore entity using Fluent API.
/// Defines table schema, indexes, constraints, and relationships for PostgreSQL.
/// </summary>
/// <remarks>
/// Hexagonal Architecture: Infrastructure layer configures domain entities without polluting them.
/// Table name: model_benchmark_scores (snake_case per PostgreSQL conventions).
/// Join entity between Model and Benchmark with additional score metadata.
/// </remarks>
public class BenchmarkScoreConfiguration : IEntityTypeConfiguration<BenchmarkScore>
{
    public void Configure(EntityTypeBuilder<BenchmarkScore> builder)
    {
        // Table name (snake_case)
        builder.ToTable("model_benchmark_scores");

        // Primary Key
        builder.HasKey(bs => bs.Id);

        // Foreign Key Indexes (for join performance and aggregations)
        builder.HasIndex(bs => bs.ModelId)
            .HasDatabaseName("idx_scores_model");

        builder.HasIndex(bs => bs.BenchmarkId)
            .HasDatabaseName("idx_scores_benchmark");

        // Composite Unique Constraint (one score per model per benchmark)
        builder.HasIndex(bs => new { bs.ModelId, bs.BenchmarkId })
            .IsUnique()
            .HasDatabaseName("unique_model_benchmark");

        // Column Configurations - Required Fields
        builder.Property(bs => bs.ModelId)
            .IsRequired();

        builder.Property(bs => bs.BenchmarkId)
            .IsRequired();

        builder.Property(bs => bs.Score)
            .HasColumnType("decimal(6,2)")
            .IsRequired();

        builder.Property(bs => bs.Verified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(bs => bs.CreatedAt)
            .IsRequired();

        // Column Configurations - Decimal Precision
        builder.Property(bs => bs.MaxScore)
            .HasColumnType("decimal(6,2)")
            .IsRequired(false);

        builder.Property(bs => bs.NormalizedScore)
            .HasColumnType("decimal(5,4)")
            .IsRequired(false);

        // Column Configurations - Optional Fields
        builder.Property(bs => bs.TestDate)
            .IsRequired(false);

        builder.Property(bs => bs.SourceUrl)
            .HasMaxLength(500);

        builder.Property(bs => bs.Notes)
            .HasMaxLength(1000);

        // Relationships are configured in ModelConfiguration and BenchmarkConfiguration (inverse sides)
        // This avoids duplicate relationship configuration
    }
}
