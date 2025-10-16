using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LlmTokenPrice.Infrastructure.Data;

/// <summary>
/// Handles database initialization and migration application.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Initializes the database by applying pending migrations.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="logger">Optional logger for diagnostic messages.</param>
    /// <remarks>
    /// In development: Creates database if it doesn't exist and applies migrations.
    /// In production: Only applies pending migrations (assumes database exists).
    /// Seed data will be added in Story 1.9.
    /// </remarks>
    public static async Task InitializeAsync(AppDbContext context, ILogger? logger = null)
    {
        try
        {
            logger?.LogInformation("Initializing database...");

            // Apply any pending migrations
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                logger?.LogInformation("Applying {Count} pending migration(s)...", pendingMigrations.Count());
                await context.Database.MigrateAsync();
                logger?.LogInformation("Migrations applied successfully");
            }
            else
            {
                logger?.LogInformation("Database is up to date, no migrations to apply");
            }

            // Seed data will be called here in Story 1.9
            // await SeedDataAsync(context, logger);

            logger?.LogInformation("Database initialization completed");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "An error occurred while initializing the database");
            throw;
        }
    }
}
