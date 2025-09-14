// OnlineBankingApp.Application/Dtos/BankAccounts/CreateBankAccountDto.cs
using System.ComponentModel.DataAnnotations;

namespace OnlineBankingApp.Application.Dtos.BankAccounts
{
    public class CreateBankAccountDto
    {
        [Required, StringLength(34, MinimumLength = 8)]
        public string IBAN { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string AccountHolder { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal WarnLimit { get; set; } = 50;

        [Required, StringLength(50)]
        public string Kontotyp { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Abteilung { get; set; } = string.Empty;
    }
}
