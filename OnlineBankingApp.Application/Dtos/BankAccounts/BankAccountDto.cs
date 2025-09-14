namespace OnlineBankingApp.Application.Dtos.BankAccounts
{
    public class BankAccountDto
    {
        public int Id { get; set; }
        public string IBAN { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string AccountHolder { get; set; } = string.Empty;

        // Optional: Falls im AdminPanel benötigt
        public decimal Balance { get; set; } = 0;
        public decimal WarnLimit { get; set; } = 50;
        // NEU
        public string Kontotyp { get; set; } = string.Empty;
        public string Abteilung { get; set; } = string.Empty;
    }
}
