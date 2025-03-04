using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace Marelli.Infra.Migrations
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public partial class changeBuildingStateColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LinkingDate",
                table: "BuildingState",
                newName: "UpdateIdsDate");

            migrationBuilder.RenameColumn(
                name: "Linking",
                table: "BuildingState",
                newName: "UpdateIds");

            migrationBuilder.RenameColumn(
                name: "IntegratingDate",
                table: "BuildingState",
                newName: "RteGenDate");

            migrationBuilder.RenameColumn(
                name: "Integrating",
                table: "BuildingState",
                newName: "RteGen");

            migrationBuilder.RenameColumn(
                name: "BuildingKitfileDate",
                table: "BuildingState",
                newName: "ParametersGenDate");

            migrationBuilder.RenameColumn(
                name: "BuildingKitfile",
                table: "BuildingState",
                newName: "ParametersGen");

            migrationBuilder.AddColumn<string>(
                name: "ApplGen",
                table: "BuildingState",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApplGenDate",
                table: "BuildingState",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DiagnoseGen",
                table: "BuildingState",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DiagnoseGenDate",
                table: "BuildingState",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "NetworkGen",
                table: "BuildingState",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "NetworkGenDate",
                table: "BuildingState",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "NvmGen",
                table: "BuildingState",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "NvmGenDate",
                table: "BuildingState",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplGen",
                table: "BuildingState");

            migrationBuilder.DropColumn(
                name: "ApplGenDate",
                table: "BuildingState");

            migrationBuilder.DropColumn(
                name: "DiagnoseGen",
                table: "BuildingState");

            migrationBuilder.DropColumn(
                name: "DiagnoseGenDate",
                table: "BuildingState");

            migrationBuilder.DropColumn(
                name: "NetworkGen",
                table: "BuildingState");

            migrationBuilder.DropColumn(
                name: "NetworkGenDate",
                table: "BuildingState");

            migrationBuilder.DropColumn(
                name: "NvmGen",
                table: "BuildingState");

            migrationBuilder.DropColumn(
                name: "NvmGenDate",
                table: "BuildingState");

            migrationBuilder.RenameColumn(
                name: "UpdateIdsDate",
                table: "BuildingState",
                newName: "LinkingDate");

            migrationBuilder.RenameColumn(
                name: "UpdateIds",
                table: "BuildingState",
                newName: "Linking");

            migrationBuilder.RenameColumn(
                name: "RteGenDate",
                table: "BuildingState",
                newName: "IntegratingDate");

            migrationBuilder.RenameColumn(
                name: "RteGen",
                table: "BuildingState",
                newName: "Integrating");

            migrationBuilder.RenameColumn(
                name: "ParametersGenDate",
                table: "BuildingState",
                newName: "BuildingKitfileDate");

            migrationBuilder.RenameColumn(
                name: "ParametersGen",
                table: "BuildingState",
                newName: "BuildingKitfile");
        }
    }
}
