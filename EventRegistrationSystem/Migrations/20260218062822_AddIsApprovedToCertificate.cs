using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventRegistrationSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddIsApprovedToCertificate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Certificates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckedOutAt",
                table: "Attendances",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckedOutBy",
                table: "Attendances",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "CheckedOutAt",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CheckedOutBy",
                table: "Attendances");
        }
    }
}
