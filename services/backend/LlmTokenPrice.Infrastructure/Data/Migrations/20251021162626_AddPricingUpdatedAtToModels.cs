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
