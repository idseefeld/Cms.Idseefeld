using System.ComponentModel.DataAnnotations;

namespace Cms.Idseefeld.Core.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Bitte geben Sie Ihren Benutzernamen ein.")]
        [Display(Name = "Benutzername")]
        [StringLength(256)]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Bitte geben Sie Ihr Passwort ein.")]
        [Display(Name = "Passwort")]
        [DataType(DataType.Password)]
        [StringLength(256)]
        public string? Password { get; set; }

        public string? RedirectUrl { get; set; }
        [Display(Name = "Angemeldet bleiben?")]
        public bool RememberMe { get; set; }
    }
}
