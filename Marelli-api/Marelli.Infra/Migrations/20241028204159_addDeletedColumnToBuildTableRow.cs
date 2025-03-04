using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace Marelli.Infra.Migrations
{
    [ExcludeFromCodeCoverage]
    /// <inheritdoc />
    public partial class addDeletedColumnToBuildTableRow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Disabled",
                table: "BuildTableRow");

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "BuildTableRow",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "BuildTableRow");

            migrationBuilder.AddColumn<bool>(
                name: "Disabled",
                table: "BuildTableRow",
                type: "boolean",
                nullable: true);
        }
    }
}
