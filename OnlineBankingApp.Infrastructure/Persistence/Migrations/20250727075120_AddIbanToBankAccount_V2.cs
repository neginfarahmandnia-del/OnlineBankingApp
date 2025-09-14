using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineBankingApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIbanToBankAccount_V2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IBAN",
                table: "BankAccounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IBAN",
                table: "BankAccounts");
        }
    }
}
