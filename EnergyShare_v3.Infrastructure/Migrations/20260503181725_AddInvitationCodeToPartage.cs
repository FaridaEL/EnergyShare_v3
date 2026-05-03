using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnergyShare_v3.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvitationCodeToPartage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InvitationCode",
                table: "partages",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InvitationCodeExpiresAt",
                table: "partages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_partages_InvitationCode",
                table: "partages",
                column: "InvitationCode",
                unique: true,
                filter: "[InvitationCode] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_partages_InvitationCode",
                table: "partages");

            migrationBuilder.DropColumn(
                name: "InvitationCode",
                table: "partages");

            migrationBuilder.DropColumn(
                name: "InvitationCodeExpiresAt",
                table: "partages");
        }
    }
}
