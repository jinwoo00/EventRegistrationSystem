using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventRegistrationSystem.Migrations
{
    /// <inheritdoc />
    public partial class FixCertificateRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EventId1",
                table: "Certificates",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_EventId1",
                table: "Certificates",
                column: "EventId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificates_Events_EventId1",
                table: "Certificates",
                column: "EventId1",
                principalTable: "Events",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Certificates_Events_EventId1",
                table: "Certificates");

            migrationBuilder.DropIndex(
                name: "IX_Certificates_EventId1",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "EventId1",
                table: "Certificates");
        }
    }
}
