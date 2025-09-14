using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineBankingApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddKontotypUndAbteilungToBankAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Abteilung",
                table: "BankAccounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Kontotyp",
                table: "BankAccounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Abteilung",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "Kontotyp",
                table: "BankAccounts");
        }
    }
}
