using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vortex_API.Migrations
{
    /// <inheritdoc />
    public partial class AddMomoOrderIdToPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MomoOrderId",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MomoOrderId",
                table: "Payments");
        }
    }
}
