using Our.Umbraco.DictionaryLocalizer.Models;
using System.ComponentModel.DataAnnotations;

namespace Cms.Idseefeld.Core.Models
{
    public class LoginModel : DictionaryDataAnnotationBaseModel
    {
        [Required(ErrorMessage = "#RequiredErrorMessage.username")]
        [Display(Name = "#usernameLabel")]
        [StringLength(256)]
        public string? Username { get; set; }

        [Required(ErrorMessage = "#RequiredErrorMessage.password")]
        [Display(Name = "#passwordLabel")]
        [DataType(DataType.Password)]
        [StringLength(256)]
        public string? Password { get; set; }

        public string? RedirectUrl { get; set; }
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
