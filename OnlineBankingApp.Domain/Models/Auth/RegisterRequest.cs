using System.ComponentModel.DataAnnotations;

namespace OnlineBankingApp.API.Models.Auth
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Benutzername ist erforderlich.")]
        public string Benutzername { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-Mail ist erforderlich.")]
        [EmailAddress(ErrorMessage = "Ungültige E-Mail-Adresse.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Passwort ist erforderlich.")]
        [MinLength(6, ErrorMessage = "Das Passwort muss mindestens 6 Zeichen lang sein.")]
        public string Passwort { get; set; } = string.Empty;

        [Required(ErrorMessage = "Passwortbestätigung ist erforderlich.")]
        [Compare("Passwort", ErrorMessage = "Die Passwörter stimmen nicht überein.")]
        public string PasswortBestaetigen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rolle ist erforderlich.")]
        public string Rolle { get; set; } = string.Empty;
    }
}
