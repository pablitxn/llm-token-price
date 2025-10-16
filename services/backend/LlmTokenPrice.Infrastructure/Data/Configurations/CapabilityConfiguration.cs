using LlmTokenPrice.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LlmTokenPrice.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for the Capability entity using Fluent API.
/// Defines table schema, indexes, constraints, and relationships for PostgreSQL.
/// </summary>
/// <remarks>
/// Hexagonal Architecture: Infrastructure layer configures domain entities without polluting them.
/// Table name: model_capabilities (snake_case per PostgreSQL conventions).
/// One-to-one relationship with Model enforced via unique constraint on ModelId.
/// </remarks>
public class CapabilityConfiguration : IEntityTypeConfiguration<Capability>
{
    public void Configure(EntityTypeBuilder<Capability> builder)
    {
        // Table name (snake_case)
        builder.ToTable("model_capabilities");

        // Primary Key
        builder.HasKey(c => c.Id);

        // Foreign Key Index (for join performance)
        builder.HasIndex(c => c.ModelId)
            .HasDatabaseName("idx_capabilities_model");

        // Unique Constraint (enforces one-to-one relationship)
        builder.HasIndex(c => c.ModelId)
            .IsUnique()
            .HasDatabaseName("unique_model_capability");

        // Column Configurations - Required Fields
        builder.Property(c => c.ModelId)
            .IsRequired();

        builder.Property(c => c.ContextWindow)
            .IsRequired();

        builder.Property(c => c.SupportsFunctionCalling)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.SupportsVision)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.SupportsAudioInput)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.SupportsAudioOutput)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.SupportsStreaming)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.SupportsJsonMode)
            .IsRequired()
            .HasDefaultValue(false);

        // Column Configurations - Optional Fields
        builder.Property(c => c.MaxOutputTokens)
            .IsRequired(false);

        // Relationships are configured in ModelConfiguration (inverse side)
        // This avoids duplicate relationship configuration
    }
}
