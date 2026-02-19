using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventRegistrationSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddEventImageAndPublish : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "Registrations");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "Events",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "Events");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Registrations",
                type: "decimal(18,2)",
                nullable: true);
        }
    }
}
