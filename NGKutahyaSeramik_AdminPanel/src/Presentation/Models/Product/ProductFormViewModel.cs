using System.ComponentModel.DataAnnotations;
using Application.Common;
using Application.ProductImages;
using Application.Products;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Presentation.Models.Product;

public class ProductTranslationFieldViewModel
{
    public int LanguageId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string LanguageName { get; set; } = string.Empty;

    [Display(Name = "Ürün Adı")]
    public string? Name { get; set; }

    [Display(Name = "Başlık")]
    public string? ShortDescription { get; set; }

    [Display(Name = "Açıklama")]
    public string? LongDescription { get; set; }

    [Display(Name = "SEO URL")]
    public string? SeoUrl { get; set; }

    [Display(Name = "Meta Başlık")]
    public string? MetaTitle { get; set; }

    [Display(Name = "Meta Açıklama")]
    public string? MetaDescription { get; set; }
}

public class ProductCreatableComboboxViewModel
{
    public string FieldName { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string? Value { get; init; }
    public IReadOnlyList<string> Options { get; init; } = Array.Empty<string>();
    public string? EmptyOptionLabel { get; init; }
    public string Placeholder { get; init; } = "Seçin veya yeni değer yazın";
}

public class ProductCreatableMultiComboboxViewModel
{
    public string FieldName { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string? Value { get; init; }
    public IReadOnlyList<string> Options { get; init; } = Array.Empty<string>();
    public string Placeholder { get; init; } = "Seçin veya yeni değer yazın";
}

public class ProductFormViewModel
{
    public int? Id { get; set; }

    [Display(Name = "Ürün Kodu")]
    public string? ProductCode { get; set; }

    [Display(Name = "Kategori")]
    public int? CategoryId { get; set; }

    [Display(Name = "Koleksiyon (Seri)")]
    public int CollectionId { get; set; }

    [Display(Name = "Marka")]
    public ProductBrand Brand { get; set; }

    [Display(Name = "Marka")]
    [MinLength(1, ErrorMessage = "En az bir marka seçmelisiniz.")]
    public List<ProductBrand> Brands { get; set; } = [];

    [Display(Name = "Durum")]
    public ProductStatus Status { get; set; }

    [Display(Name = "Ürün Grubu")]
    public string? ProductGroup { get; set; }

    [Display(Name = "Ebat")]
    public string? Size { get; set; }

    [Display(Name = "Satış Birimi")]
    public string? Unit { get; set; }

    [Display(Name = "Yüzey")]
    public string? Surface { get; set; }

    [Display(Name = "Yüzey")]
    public int? SurfaceId { get; set; }

    [Display(Name = "Rölyef")]
    public string? Relief { get; set; }

    [Display(Name = "Özel Yüzey")]
    public string? SpecialSurface { get; set; }

    [Display(Name = "Face Görsel Adedi")]
    [Range(1, int.MaxValue, ErrorMessage = "Face görsel adedi pozitif tam sayı olmalıdır.")]
    public int? FaceCount { get; set; }

    [Display(Name = "Face")]
    public bool? HasFace { get; set; }

    [Display(Name = "Mekân")]
    public bool? HasVenue { get; set; }

    [Display(Name = "Kalınlık (mm)")]
    public decimal? Thickness { get; set; }

    [Display(Name = "Bünye")]
    public string? BodyType { get; set; }

    [Display(Name = "Renk")]
    public string? Color { get; set; }

    [Display(Name = "Renk Malzeme Grubu")]
    public string? ColorMaterial { get; set; }

    [Display(Name = "Uygulama Alanı")]
    public string? ApplicationArea { get; set; }

    [Display(Name = "Kullanım Alanı")]
    public string? UsageArea { get; set; }

    [Display(Name = "Bitiş")]
    public string? Finish { get; set; }

    [Display(Name = "PEI")]
    [Range(1, 5, ErrorMessage = "PEI değeri 1 ile 5 arasında olmalıdır.")]
    public decimal? PEI { get; set; }

    [Display(Name = "V Değeri")]
    [RegularExpression(@"^V[1-4]$", ErrorMessage = "V değeri V1 ile V4 arasında olmalıdır.")]
    public string? VValue { get; set; }

    [Display(Name = "R Değeri")]
    [RegularExpression(@"^(R9|R10|R11|R12|R13|R11-R12)$", ErrorMessage = "R değeri R9, R10, R11, R12, R13 veya R11-R12 olmalıdır.")]
    public string? RValue { get; set; }

    [Display(Name = "Derin Aşınma")]
    public string? DeepAbrasion { get; set; }

    [Display(Name = "Isıya Dayanıklılık")]
    public bool? HeatResistance { get; set; }

    [Display(Name = "Kaymaz Özellik")]
    public bool? AntiSlip { get; set; }

    [Display(Name = "Sırlı Granite")]
    public bool? GlazedGranite { get; set; }

    [Display(Name = "Kutu m²")]
    public decimal? BoxM2 { get; set; }

    [Display(Name = "Palet m²")]
    public decimal? PalletM2 { get; set; }
    [Display(Name = "Sıralama")]
    public int? DisplayOrder { get; set; }

    public List<ProductTranslationFieldViewModel> Translations { get; set; } = [];

    public List<SelectListItem> CategoryOptions { get; set; } = [];
    public List<SelectListItem> CollectionOptions { get; set; } = [];
    public List<SelectListItem> SurfaceOptions { get; set; } = [];
    public List<SelectListItem> BrandOptions { get; set; } = [];
    public List<SelectListItem> StatusOptions { get; set; } = [];
    public ProductTechnicalOptionsDto TechnicalOptions { get; set; } = new();

    public List<ProductImageDto> Images { get; set; } = [];
    public List<SelectListItem> ImageTypeOptions { get; set; } = [];
    public ProductImageType InitialImageType { get; set; }
    public List<IFormFile> InitialImageFiles { get; set; } = [];

    // Alan-seviyeli RBAC (backlog #23) — Controller tarafından role göre set edilir; View bu
    // bayraklara göre ilgili alanları disabled yapar. Varsayılan true: Create ekranı yalnızca
    // Admin/Ürün Yöneticisi'ne açık olduğu için orada hiç kısıtlama uygulanmaz.
    public bool CanEditNativeFields { get; set; } = true;
    public bool CanEditContentFields { get; set; } = true;
    public bool CanEditSeoFields { get; set; } = true;
}

public class ProductIndexViewModel
{
    public ProductPagedResult<ProductListItemDto> Page { get; init; } = new();
    public string Sort { get; init; } = ProductSortOptions.DisplayOrder;
    public List<SelectListItem> SortOptions { get; init; } = [];
}
