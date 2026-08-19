using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Application.ProductImport;

public static partial class ProductImportHeaders
{
    public const string ProductCode = nameof(ProductCode);
    public const string ProductName = nameof(ProductName);
    public const string Series = nameof(Series);
    public const string Brand = nameof(Brand);
    public const string CommercialName = nameof(CommercialName);
    public const string Status = nameof(Status);
    public const string ProductGroup = nameof(ProductGroup);
    public const string Size = nameof(Size);
    public const string Unit = nameof(Unit);
    public const string Surface = nameof(Surface);
    public const string Relief = nameof(Relief);
    public const string SpecialSurface = nameof(SpecialSurface);
    public const string HasFace = nameof(HasFace);
    public const string HasVenue = nameof(HasVenue);
    public const string FaceCount = nameof(FaceCount);
    public const string Category = nameof(Category);
    public const string Thickness = nameof(Thickness);
    public const string BodyType = nameof(BodyType);
    public const string VValue = nameof(VValue);
    public const string PEI = nameof(PEI);
    public const string RValue = nameof(RValue);
    public const string DeepAbrasion = nameof(DeepAbrasion);
    public const string Color = nameof(Color);
    public const string ColorMaterial = nameof(ColorMaterial);
    public const string Finish = nameof(Finish);
    public const string HeatResistance = nameof(HeatResistance);
    public const string AntiSlip = nameof(AntiSlip);
    public const string ApplicationArea = nameof(ApplicationArea);
    public const string UsageArea = nameof(UsageArea);
    public const string GlazedGranite = nameof(GlazedGranite);
    public const string BoxM2 = nameof(BoxM2);
    public const string PalletM2 = nameof(PalletM2);

    public static readonly IReadOnlyList<string> CanonicalHeaders =
    [
        ProductCode, Series, Brand, ProductName, CommercialName, Status, ProductGroup, Size,
        Unit, Surface, Relief, SpecialSurface, HasFace, HasVenue, FaceCount, Category,
        Thickness, BodyType, VValue, PEI, RValue, DeepAbrasion, Color, ColorMaterial,
        Finish, HeatResistance, AntiSlip, ApplicationArea, UsageArea, GlazedGranite, BoxM2, PalletM2
    ];

    public static readonly IReadOnlyDictionary<string, string> TurkishHeaderByCanonical = new Dictionary<string, string>
    {
        [ProductCode] = "Ürün Kodu",
        [Series] = "Seri Adı",
        [Brand] = "Ürün Segmentasyonu",
        [ProductName] = "Ürün Adı",
        [CommercialName] = "Koleksiyon İsimleri",
        [Status] = "Durum",
        [ProductGroup] = "Ürün Grubu",
        [Size] = "Ebat",
        [Unit] = "Birim",
        [Surface] = "Yüzey",
        [Relief] = "Rölyef",
        [SpecialSurface] = "Özel Yüzey",
        [HasFace] = "Face Görseli",
        [HasVenue] = "Mekân Görseli",
        [FaceCount] = "Face Sayısı",
        [Category] = "Kategori",
        [Thickness] = "Kalınlık (mm)",
        [BodyType] = "Bünye",
        [VValue] = "V Değeri",
        [PEI] = "PEI Değeri",
        [RValue] = "R Değeri",
        [DeepAbrasion] = "Derin Aşınma",
        [Color] = "Renk",
        [ColorMaterial] = "Malzeme Rengi",
        [Finish] = "Bitiş",
        [HeatResistance] = "Isıya Dayanıklılık",
        [AntiSlip] = "Kaymazlık",
        [ApplicationArea] = "Uygulama Alanı",
        [UsageArea] = "Kullanım Alanı",
        [GlazedGranite] = "Sırlı Granit",
        [BoxM2] = "Kutu m²",
        [PalletM2] = "Palet m²"
    };

    private static readonly IReadOnlyDictionary<string, string> AliasByNormalizedHeader = BuildAliases();

    public static string? ResolveCanonical(string? header)
    {
        var normalized = NormalizeHeader(header);
        return normalized is not null && AliasByNormalizedHeader.TryGetValue(normalized, out var canonical)
            ? canonical
            : null;
    }

    public static string DisplayName(string canonical) =>
        TurkishHeaderByCanonical.TryGetValue(canonical, out var label) ? label : canonical;

    public static string? NormalizeHeader(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = SpaceRegex().Replace(value.Trim(), " ");
        return normalized.ToUpper(new CultureInfo("tr-TR"));
    }

    private static IReadOnlyDictionary<string, string> BuildAliases()
    {
        var map = new Dictionary<string, string>();

        void Add(string canonical, params string[] aliases)
        {
            foreach (var alias in aliases.Append(canonical))
            {
                var normalized = NormalizeHeader(alias);
                if (normalized is not null)
                {
                    map[normalized] = canonical;
                }
            }
        }

        Add(ProductCode, "Ürün Kodu");
        Add(ProductName, "Ürün Adı");
        Add(Series, "Seri Adı");
        Add(Brand, "Ürün Segmantasyonu", "Ürün Segmentasyonu");
        Add(CommercialName, "Koleksiyon İsimleri");
        Add(Status, "Durum");
        Add(ProductGroup, "Ürün Grubu");
        Add(Size, "Ebat");
        Add(Unit, "Brm", "Birim", "M2");
        Add(Surface, "Yüzey");
        Add(Relief, "Rölyef");
        Add(SpecialSurface, "Özel Yüzey");
        Add(HasFace, "Face", "Face Görseli");
        Add(HasVenue, "Mekan", "Mekân", "Mekân Görseli");
        Add(FaceCount, "Face Sayısı");
        Add(Category, "Kategori");
        Add(Thickness, "Kalınlık (mm)");
        Add(BodyType, "Bünye");
        Add(VValue, "V değeri", "V Değeri");
        Add(PEI, "PEI Değeri");
        Add(RValue, "R Değeri");
        Add(DeepAbrasion, "Derin Aşınma");
        Add(Color, "Renk", "Renk (Ürün Tanımındaki)");
        Add(ColorMaterial, "Renk Mlz", "Malzeme Rengi");
        Add(Finish, "Bitiş");
        Add(HeatResistance, "Isıya Dayanıklılık");
        Add(AntiSlip, "Kaymaz", "Kaymazlık");
        Add(ApplicationArea, "Uygulama Alanı", "Uygulama Alanı (YER-DUVAR)");
        Add(UsageArea, "Kullanım Alanı", "Kullanım Alanı (Banyo-Mutfak)");
        Add(GlazedGranite, "Sırlı Granite", "Sırlı Granit");
        Add(BoxM2, "Kutu m²", "Kutu m2");
        Add(PalletM2, "Palet m²", "Palet m2");

        return map;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpaceRegex();
}
