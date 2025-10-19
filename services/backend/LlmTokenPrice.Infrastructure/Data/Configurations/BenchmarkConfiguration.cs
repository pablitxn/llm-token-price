using LlmTokenPrice.Domain.Entities;
using LlmTokenPrice.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LlmTokenPrice.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for the Benchmark entity using Fluent API.
/// Defines table schema, indexes, constraints, and relationships for PostgreSQL.
/// </summary>
/// <remarks>
/// Hexagonal Architecture: Infrastructure layer configures domain entities without polluting them.
/// Table name: benchmarks (snake_case per PostgreSQL conventions).
/// </remarks>
public class BenchmarkConfiguration : IEntityTypeConfiguration<Benchmark>
{
    public void Configure(EntityTypeBuilder<Benchmark> builder)
    {
        // Table name (snake_case)
        builder.ToTable("benchmarks");

        // Primary Key
        builder.HasKey(b => b.Id);

        // Unique Constraint on benchmark name
        builder.HasIndex(b => b.BenchmarkName)
            .IsUnique()
            .HasDatabaseName("unique_benchmark_name");

        // Column Configurations - Required Fields
        builder.Property(b => b.BenchmarkName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.CreatedAt)
            .IsRequired();

        // Column Configurations - Optional Fields
        builder.Property(b => b.FullName)
            .HasMaxLength(300);

        builder.Property(b => b.Description)
            .HasMaxLength(1000);

        // Enum stored as string in database
        builder.Property(b => b.Category)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(b => b.Interpretation)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Column Configurations - Decimal Precision for typical ranges
        builder.Property(b => b.TypicalRangeMin)
            .HasColumnType("decimal(5,2)")
            .IsRequired(false);

        builder.Property(b => b.TypicalRangeMax)
            .HasColumnType("decimal(5,2)")
            .IsRequired(false);

        // Column Configuration - QAPS Weight
        builder.Property(b => b.WeightInQaps)
            .HasColumnType("decimal(3,2)")
            .IsRequired()
            .HasDefaultValue(0m);

        // Column Configuration - Soft Delete Flag
        builder.Property(b => b.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Index on IsActive for public queries (filtering active benchmarks)
        builder.HasIndex(b => b.IsActive)
            .HasDatabaseName("idx_benchmarks_is_active");

        // Relationships

        // One-to-many: Benchmark -> BenchmarkScores (cascade delete)
        builder.HasMany(b => b.Scores)
            .WithOne(bs => bs.Benchmark)
            .HasForeignKey(bs => bs.BenchmarkId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
