using System.ComponentModel.DataAnnotations;

namespace OnlineBankingAdminPanel.Models.Auth
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Benutzername ist erforderlich.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-Mail ist erforderlich.")]
        [EmailAddress(ErrorMessage = "Ungültige E-Mail-Adresse.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Passwort ist erforderlich.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bitte Passwort bestätigen.")]
        [Compare("Password", ErrorMessage = "Passwörter stimmen nicht überein.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

}
