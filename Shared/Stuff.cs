using System.ComponentModel.DataAnnotations;

namespace IdentityWithCookies.Shared
{
    public class RegisterAccountFormDto
    {
        [EmailAddress]
        [Required]
        [Display(Name="Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name="Password")]
        public string Password { get; set; }
    }

    public class LoginAccountFormDto
    {
        [EmailAddress]
        [Required]
        [Display(Name="Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name="Password")]
        public string Password { get; set; }
    }

    public enum AuthorizationRoles
    {
        Admin,
        User
    }
}
