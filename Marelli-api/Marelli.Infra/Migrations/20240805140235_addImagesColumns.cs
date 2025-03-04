using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace Marelli.Infra.Migrations
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public partial class addImagesColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string[]>(
                name: "CompaniesImages",
                table: "Group",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "Group",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompaniesImages",
                table: "Group");

            migrationBuilder.DropColumn(
                name: "Image",
                table: "Group");
        }
    }
}
