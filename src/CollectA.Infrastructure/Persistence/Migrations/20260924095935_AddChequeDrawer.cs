using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CollectA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddChequeDrawer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Drawer",
                table: "Cheques",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Drawer",
                table: "Cheques");
        }
    }
}
