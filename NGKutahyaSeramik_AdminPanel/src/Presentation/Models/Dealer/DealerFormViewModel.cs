using System.ComponentModel.DataAnnotations;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Presentation.Models.Dealer;

public class DealerFormViewModel
{
    public int? Id { get; set; }

    [Display(Name = "İsim")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Kategori")]
    [Required(ErrorMessage = "Kategori seçiniz.")]
    public DealerCategory? Category { get; set; }

    [Display(Name = "Ülke")]
    public string City { get; set; } = string.Empty;

    [Display(Name = "Şehir")]
    public string? District { get; set; }

    [Display(Name = "Adres")]
    public string? Address { get; set; }

    [Display(Name = "Telefon")]
    public string? Phone { get; set; }

    [Display(Name = "Faks")]
    public string? Fax { get; set; }

    [Display(Name = "E-posta")]
    public string? Email { get; set; }

    [Display(Name = "Çalışma Saatleri")]
    public string? WorkingHours { get; set; }

    [Display(Name = "Enlem")]
    public decimal? Latitude { get; set; }

    [Display(Name = "Boylam")]
    public decimal? Longitude { get; set; }

    [Display(Name = "Markalar")]
    public List<ProductBrand> Brands { get; set; } = Enum.GetValues<ProductBrand>().ToList();

    public List<SelectListItem> CategoryOptions { get; set; } = [];
}
