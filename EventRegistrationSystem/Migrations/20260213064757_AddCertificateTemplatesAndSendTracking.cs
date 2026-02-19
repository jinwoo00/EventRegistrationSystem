using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventRegistrationSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddCertificateTemplatesAndSendTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTemplateBased",
                table: "Certificates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SentAt",
                table: "Certificates",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SentToEmail",
                table: "Certificates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CertificateTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificateTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CertificateTemplates_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CertificateTemplates_EventId",
                table: "CertificateTemplates",
                column: "EventId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CertificateTemplates");

            migrationBuilder.DropColumn(
                name: "IsTemplateBased",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "SentAt",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "SentToEmail",
                table: "Certificates");
        }
    }
}
