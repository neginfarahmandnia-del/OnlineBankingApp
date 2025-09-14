using System.ComponentModel.DataAnnotations;

namespace OnlineBankingAdminPanel.Models
{
    public class BankkontoCreateViewModel
    {
        [Required]
        public string Kontoinhaber { get; set; } = string.Empty;

        [Required]
        public int KontoTypId { get; set; }

        [Required]
        public int AbteilungId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Startguthaben { get; set; }

        public List<KontoTyp> KontoTypen { get; set; } = new();
        public List<Abteilung> Abteilungen { get; set; } = new();
    }

    public class KontoTyp
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class Abteilung
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

}
