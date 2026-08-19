using System.ComponentModel.DataAnnotations;

namespace Presentation.Models.Account;

public class LoginViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Parola")]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
