using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LlmTokenPrice.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPricingUpdatedAtToModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PricingUpdatedAt",
                table: "models",
                type: "timestamp with time zone",
                nullable: true);

            // Backfill existing models: set pricing_updated_at = updated_at
            // This ensures existing models have a pricing freshness baseline
            migrationBuilder.Sql(@"
                UPDATE models
                SET ""PricingUpdatedAt"" = ""UpdatedAt""
                WHERE ""PricingUpdatedAt"" IS NULL;
            ");

            migrationBuilder.CreateIndex(
                name: "idx_models_pricing_updated",
                table: "models",
                column: "PricingUpdatedAt",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_models_pricing_updated",
                table: "models");

            migrationBuilder.DropColumn(
                name: "PricingUpdatedAt",
                table: "models");
        }
    }
}
