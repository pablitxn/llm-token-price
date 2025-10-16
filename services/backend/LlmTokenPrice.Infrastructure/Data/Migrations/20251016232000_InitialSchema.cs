using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LlmTokenPrice.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "benchmarks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BenchmarkName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FullName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Interpretation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TypicalRangeMin = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    TypicalRangeMax = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_benchmarks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "models",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Provider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ReleaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "active"),
                    InputPricePer1M = table.Column<decimal>(type: "numeric(10,6)", nullable: false),
                    OutputPricePer1M = table.Column<decimal>(type: "numeric(10,6)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    PricingValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PricingValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastScrapedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_models", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "model_benchmark_scores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    BenchmarkId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<decimal>(type: "numeric(6,2)", nullable: false),
                    MaxScore = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    NormalizedScore = table.Column<decimal>(type: "numeric(5,4)", nullable: true),
                    TestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SourceUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Verified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_model_benchmark_scores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_model_benchmark_scores_benchmarks_BenchmarkId",
                        column: x => x.BenchmarkId,
                        principalTable: "benchmarks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_model_benchmark_scores_models_ModelId",
                        column: x => x.ModelId,
                        principalTable: "models",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "model_capabilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContextWindow = table.Column<int>(type: "integer", nullable: false),
                    MaxOutputTokens = table.Column<int>(type: "integer", nullable: true),
                    SupportsFunctionCalling = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SupportsVision = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SupportsAudioInput = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SupportsAudioOutput = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SupportsStreaming = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SupportsJsonMode = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_model_capabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_model_capabilities_models_ModelId",
                        column: x => x.ModelId,
                        principalTable: "models",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "unique_benchmark_name",
                table: "benchmarks",
                column: "BenchmarkName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_scores_benchmark",
                table: "model_benchmark_scores",
                column: "BenchmarkId");

            migrationBuilder.CreateIndex(
                name: "idx_scores_model",
                table: "model_benchmark_scores",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "unique_model_benchmark",
                table: "model_benchmark_scores",
                columns: new[] { "ModelId", "BenchmarkId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "unique_model_capability",
                table: "model_capabilities",
                column: "ModelId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_models_provider",
                table: "models",
                column: "Provider");

            migrationBuilder.CreateIndex(
                name: "idx_models_status",
                table: "models",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "idx_models_updated",
                table: "models",
                column: "UpdatedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "unique_model_provider",
                table: "models",
                columns: new[] { "Name", "Provider" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "model_benchmark_scores");

            migrationBuilder.DropTable(
                name: "model_capabilities");

            migrationBuilder.DropTable(
                name: "benchmarks");

            migrationBuilder.DropTable(
                name: "models");
        }
    }
}
