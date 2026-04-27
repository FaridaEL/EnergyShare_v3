using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnergyShare_v3.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AjoutPointAccessProprieteActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DesactiveAt",
                table: "point_accesses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EstActif",
                table: "point_accesses",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_point_accesses_EAN_Encrypted_EstActif",
                table: "point_accesses",
                columns: new[] { "EAN_Encrypted", "EstActif" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_point_accesses_EAN_Encrypted_EstActif",
                table: "point_accesses");

            migrationBuilder.DropColumn(
                name: "DesactiveAt",
                table: "point_accesses");

            migrationBuilder.DropColumn(
                name: "EstActif",
                table: "point_accesses");
        }
    }
}
