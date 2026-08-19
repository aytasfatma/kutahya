using System.ComponentModel.DataAnnotations;

namespace Presentation.Models.User;

public class ResetPasswordViewModel
{
    public string Id { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Yeni Parola")]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Yeni Parola (Tekrar)")]
    [Compare(nameof(NewPassword), ErrorMessage = "Parolalar eşleşmiyor.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
