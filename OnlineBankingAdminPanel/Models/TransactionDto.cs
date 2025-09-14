namespace OnlineBankingAdminPanel.Models
{
    public class TransactionDto
    {
        public int Id { get; set; }
        public string IBAN { get; set; } = string.Empty;
        public string AccountHolder { get; set; } = string.Empty;
        public decimal Betrag { get; set; }
        public DateTime Datum { get; set; }
        public string Verwendungszweck { get; set; } = string.Empty;
    }
}
