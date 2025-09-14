using System.ComponentModel.DataAnnotations;

public class BankkontoDto
{
    [Required]
    public string InhaberName { get; set; } = string.Empty;

    [Required]
    public string KontoTyp { get; set; } = string.Empty;

    [Required]
    public string Abteilung { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Startguthaben { get; set; }
}
