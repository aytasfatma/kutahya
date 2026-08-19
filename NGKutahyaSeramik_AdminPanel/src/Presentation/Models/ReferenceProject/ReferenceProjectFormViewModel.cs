using System.ComponentModel.DataAnnotations;
using Application.Common;
using Application.ReferenceProjects;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Presentation.Models.ReferenceProject;

public class ReferenceProjectTranslationFieldViewModel
{
    public int LanguageId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string LanguageName { get; set; } = string.Empty;

    [Display(Name = "Proje Adı")]
    public string? ProjectName { get; set; }

    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    [Display(Name = "SEO URL")]
    public string? SeoUrl { get; set; }
}

public class ReferenceProjectFormViewModel
{
    public int? Id { get; set; }

    [Display(Name = "Lokasyon")]
    public string? Location { get; set; }

    [Required(ErrorMessage = "Bölge seçimi zorunludur.")]
    [Display(Name = "Bölge")]
    public ReferenceProjectRegion? Region { get; set; }

    [Required(ErrorMessage = "Marka seçimi zorunludur.")]
    [Display(Name = "Marka")]
    public ProductBrand? Brand { get; set; }

    [Display(Name = "Proje Tipi")]
    public ReferenceProjectType ProjectType { get; set; }

    [Display(Name = "Mimar")]
    public string? Architect { get; set; }

    [Display(Name = "Yıl")]
    [DataType(DataType.Date)]
    public DateTime? ProjectDate { get; set; }
    [Required(ErrorMessage = SortOrderValidationMessages.Required)]
    [Range(1, int.MaxValue, ErrorMessage = SortOrderValidationMessages.Minimum)]
    [Display(Name = "Sıralama")]
    public int? DisplayOrder { get; set; }

    [Display(Name = "İlişkili Ürünler")]
    public List<int> SelectedProductIds { get; set; } = [];

    public List<ReferenceProjectTranslationFieldViewModel> Translations { get; set; } = [];

    public List<SelectListItem> ProjectTypeOptions { get; set; } = [];
    public List<SelectListItem> RegionOptions { get; set; } = [];
    public List<SelectListItem> BrandOptions { get; set; } = [];
    public List<SelectListItem> ProductOptions { get; set; } = [];

    public List<ReferenceProjectImageDto> Images { get; set; } = [];
}

