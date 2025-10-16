using LlmTokenPrice.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LlmTokenPrice.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for the Model entity using Fluent API.
/// Defines table schema, indexes, constraints, and relationships for PostgreSQL.
/// </summary>
/// <remarks>
/// Hexagonal Architecture: Infrastructure layer configures domain entities without polluting them.
/// Table name: models (snake_case per PostgreSQL conventions).
/// </remarks>
public class ModelConfiguration : IEntityTypeConfiguration<Model>
{
    public void Configure(EntityTypeBuilder<Model> builder)
    {
        // Table name (snake_case)
        builder.ToTable("models");

        // Primary Key
        builder.HasKey(m => m.Id);

        // Indexes for query performance
        builder.HasIndex(m => m.Provider)
            .HasDatabaseName("idx_models_provider");

        builder.HasIndex(m => m.Status)
            .HasDatabaseName("idx_models_status");

        builder.HasIndex(m => m.UpdatedAt)
            .HasDatabaseName("idx_models_updated")
            .IsDescending(); // DESC for "recently updated" queries

        // Unique Constraints
        builder.HasIndex(m => new { m.Name, m.Provider })
            .IsUnique()
            .HasDatabaseName("unique_model_provider");

        // Column Configurations - Required Fields
        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Provider)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("active");

        builder.Property(m => m.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("USD");

        builder.Property(m => m.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(m => m.CreatedAt)
            .IsRequired();

        builder.Property(m => m.UpdatedAt)
            .IsRequired();

        // Column Configurations - Decimal Precision
        builder.Property(m => m.InputPricePer1M)
            .HasColumnType("decimal(10,6)")
            .IsRequired();

        builder.Property(m => m.OutputPricePer1M)
            .HasColumnType("decimal(10,6)")
            .IsRequired();

        // Column Configurations - Optional Fields
        builder.Property(m => m.Version)
            .HasMaxLength(50);

        builder.Property(m => m.ReleaseDate)
            .IsRequired(false);

        builder.Property(m => m.PricingValidFrom)
            .IsRequired(false);

        builder.Property(m => m.PricingValidTo)
            .IsRequired(false);

        builder.Property(m => m.LastScrapedAt)
            .IsRequired(false);

        // Relationships

        // One-to-one: Model -> Capability (cascade delete)
        builder.HasOne(m => m.Capability)
            .WithOne(c => c.Model)
            .HasForeignKey<Capability>(c => c.ModelId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-many: Model -> BenchmarkScores (cascade delete)
        builder.HasMany(m => m.BenchmarkScores)
            .WithOne(bs => bs.Model)
            .HasForeignKey(bs => bs.ModelId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
