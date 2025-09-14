using System.ComponentModel.DataAnnotations;

namespace OnlineBankingAdminPanel.Models.Auth
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "E-Mail ist erforderlich.")]
        [EmailAddress(ErrorMessage = "Ungültige E-Mail-Adresse.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Passwort ist erforderlich.")]
        public string Password { get; set; } = string.Empty;
    }


}
