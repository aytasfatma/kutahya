using System.ComponentModel.DataAnnotations;
using Application.Common;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Presentation.Models.Document;

public class DocumentFormViewModel
{
    public int? Id { get; set; }

    [Display(Name = "Doküman Adı")]
    [Required(ErrorMessage = "Doküman adı zorunludur.")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Açıklama")]
    [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
    public string? Description { get; set; }

    [Display(Name = "Doküman Tipi")]
    [Required]
    public DocumentType? DocumentType { get; set; }

    [Display(Name = "Marka")]
    [Required(ErrorMessage = "Marka seçimi zorunludur.")]
    public ProductBrand? Brand { get; set; }

    [Display(Name = "Koleksiyon")]
    [Required(ErrorMessage = "Koleksiyon seçimi zorunludur.")]
    public int? SelectedCollectionId { get; set; }

    [Display(Name = "Dil")]
    public int LanguageId { get; set; }
    [Required(ErrorMessage = SortOrderValidationMessages.Required)]
    [Range(1, int.MaxValue, ErrorMessage = SortOrderValidationMessages.Minimum)]
    [Display(Name = "Sıralama")]
    public int? DisplayOrder { get; set; }

    [Display(Name = "İlişkili Ürünler")]
    public List<int> SelectedProductIds { get; set; } = [];

    [Display(Name = "İlişkili Koleksiyonlar")]
    public List<int> SelectedCollectionIds { get; set; } = [];

    // Yalnızca Edit ekranında gösterim amaçlı (Create'te dosya zorunlu, boş gelir).
    public string? ExistingOriginalFileName { get; set; }
    public string? ExistingFilePath { get; set; }
    public string? ExistingFileSizeLabel { get; set; }

    public List<SelectListItem> DocumentTypeOptions { get; set; } = [];
    public List<SelectListItem> BrandOptions { get; set; } = [];
    public List<SelectListItem> LanguageOptions { get; set; } = [];
    public List<SelectListItem> ProductOptions { get; set; } = [];
    public List<SelectListItem> CollectionOptions { get; set; } = [];
}

