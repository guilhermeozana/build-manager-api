using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace Marelli.Infra.Migrations
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public partial class addTagDescriptionColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Tag",
                table: "BuildTableRow",
                newName: "TagName");

            migrationBuilder.AddColumn<string>(
                name: "TagDescription",
                table: "BuildTableRow",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TagDescription",
                table: "BuildTableRow");

            migrationBuilder.RenameColumn(
                name: "TagName",
                table: "BuildTableRow",
                newName: "Tag");
        }
    }
}
