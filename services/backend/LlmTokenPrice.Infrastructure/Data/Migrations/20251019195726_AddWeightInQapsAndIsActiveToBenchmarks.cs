using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LlmTokenPrice.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWeightInQapsAndIsActiveToBenchmarks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Interpretation",
                table: "benchmarks",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "benchmarks",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "benchmarks",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WeightInQaps",
                table: "benchmarks",
                type: "numeric(3,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "idx_benchmarks_is_active",
                table: "benchmarks",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_benchmarks_is_active",
                table: "benchmarks");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "benchmarks");

            migrationBuilder.DropColumn(
                name: "WeightInQaps",
                table: "benchmarks");

            migrationBuilder.AlterColumn<string>(
                name: "Interpretation",
                table: "benchmarks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "benchmarks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);
        }
    }
}
