using System.ComponentModel.DataAnnotations;

namespace Presentation.Models.User;

public class UserFormViewModel
{
    public string? Id { get; set; }

    /// <summary>Create'te düzenlenebilir giriş alanı, Edit'te salt-okunur gösterim amaçlı — [Required]
    /// kasıtlı olarak yok: Edit formu Email input'u render etmediği için boş/eski değerle post
    /// edilse dahi ModelState'i geçersiz kılmamalı (Email/UserName oluşturulduktan sonra değişmez,
    /// Task 16B revizyon kararı).</summary>
    [EmailAddress]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Yalnızca Create formunda kullanılır — Edit formunda parola alanı yer almaz (parola
    /// sıfırlama ayrı bir ekrandan yapılır, Task 16 kararı).</summary>
    [DataType(DataType.Password)]
    [Display(Name = "Parola")]
    public string? Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Parola (Tekrar)")]
    [Compare(nameof(Password), ErrorMessage = "Parolalar eşleşmiyor.")]
    public string? ConfirmPassword { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    /// <summary>Her kullanıcı tam olarak bir role sahiptir (Task 16B revizyon kararı) — dropdown/radio
    /// ile tekil seçim, çoklu rol desteği kaldırıldı.</summary>
    [Required(ErrorMessage = "Rol seçilmelidir.")]
    [Display(Name = "Rol")]
    public string Role { get; set; } = string.Empty;

    public List<string> AvailableRoles { get; set; } = [];
}
