using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineBankingApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserIdUniqueFromBankAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Alten UNIQUE-Index auf UserId entfernen
            migrationBuilder.DropIndex(
                name: "IX_BankAccounts_UserId",
                table: "BankAccounts");

            // Typ von UserId NICHT ändern – er bleibt nvarchar(450)
            // (Falls du vorher in einer anderen Migration auf nvarchar(450) umgestellt hast,
            // lassen wir das hier unverändert.)

            // Neuen UNIQUE-Index auf IBAN anlegen
            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_IBAN",
                table: "BankAccounts",
                column: "IBAN",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // IBAN-Index wieder entfernen
            migrationBuilder.DropIndex(
                name: "IX_BankAccounts_IBAN",
                table: "BankAccounts");

            // Alten UNIQUE-Index auf UserId wiederherstellen
            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_UserId",
                table: "BankAccounts",
                column: "UserId",
                unique: true);
        }
    }
}
