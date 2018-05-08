using Microsoft.EntityFrameworkCore.Migrations;

namespace Authorization.API.Migrations
{
    public partial class AddedTokenHashedField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Token",
                table: "Users",
                newName: "TokenHashed");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TokenHashed",
                table: "Users",
                newName: "Token");
        }
    }
}
