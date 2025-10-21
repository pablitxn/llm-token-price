using LlmTokenPrice.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LlmTokenPrice.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for the AuditLog entity using Fluent API.
/// Defines table schema, indexes, and constraints for PostgreSQL.
/// </summary>
/// <remarks>
/// Hexagonal Architecture: Infrastructure layer configures domain entities without polluting them.
/// Table name: audit_logs (snake_case per PostgreSQL conventions).
/// Immutable append-only pattern: no updates or deletes, only inserts.
/// </remarks>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        // Table name (snake_case)
        builder.ToTable("audit_logs");

        // Primary Key
        builder.HasKey(a => a.Id);

        // Indexes for query performance

        // Index on Timestamp (descending) for chronological queries and date range filters
        builder.HasIndex(a => a.Timestamp)
            .HasDatabaseName("idx_audit_logs_timestamp")
            .IsDescending(); // DESC for "most recent first" queries

        // Index on UserId for filtering by user
        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("idx_audit_logs_user_id");

        // Index on Action for filtering by action type (Create, Update, Delete, Import)
        builder.HasIndex(a => a.Action)
            .HasDatabaseName("idx_audit_logs_action");

        // Index on EntityType for filtering by entity type (Model, Benchmark, BenchmarkScore)
        builder.HasIndex(a => a.EntityType)
            .HasDatabaseName("idx_audit_logs_entity_type");

        // Index on EntityId for entity-level audit trail queries
        builder.HasIndex(a => a.EntityId)
            .HasDatabaseName("idx_audit_logs_entity_id");

        // Composite index for common query pattern: filter by entity type AND entity id
        builder.HasIndex(a => new { a.EntityType, a.EntityId })
            .HasDatabaseName("idx_audit_logs_entity");

        // Column Configurations - Required Fields

        builder.Property(a => a.Timestamp)
            .IsRequired()
            .HasColumnType("timestamp with time zone"); // UTC timestamps with timezone awareness

        builder.Property(a => a.UserId)
            .IsRequired()
            .HasMaxLength(200); // Supports email addresses (up to 254 chars) and usernames

        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(50); // "Create", "Update", "Delete", "Import"

        builder.Property(a => a.EntityType)
            .IsRequired()
            .HasMaxLength(100); // "Model", "Benchmark", "BenchmarkScore"

        builder.Property(a => a.EntityId)
            .IsRequired();

        // Column Configurations - Optional JSONB Fields (PostgreSQL-specific)

        // OldValues: JSON representation before the operation (NULL for Create)
        builder.Property(a => a.OldValues)
            .HasColumnType("jsonb") // PostgreSQL JSONB for compression and queryability
            .IsRequired(false);

        // NewValues: JSON representation after the operation (NULL for Delete)
        builder.Property(a => a.NewValues)
            .HasColumnType("jsonb") // PostgreSQL JSONB for compression and queryability
            .IsRequired(false);
    }
}
