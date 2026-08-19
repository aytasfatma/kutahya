namespace Application.Dashboard;

/// <summary>
/// Madde 17.2 modül #1 (Dashboard) — dokümanda kart/grafik/tablo tanımı yok, yalnızca modül adı
/// geçiyor. Bu yüzden alanlar tamamen mevcut entity/kod tarafından desteklenen, filtre anlamı açık
/// sayaçlarla sınırlı tutuldu (Task 18 analiz kararı). "Toplam içerik" gibi birleşik/belirsiz
/// sayaçlar YOK. Aktif kullanıcı hariç "son eklenen X" listeleri yalnızca CreatedAt alanı gerçekten
/// mevcut olan entity'ler için var (ApplicationUser'da CreatedAt yok — "son eklenen kullanıcılar"
/// bu yüzden kapsam dışı).
/// </summary>
public class DashboardDto
{
    public int TotalProducts { get; init; }

    /// <summary>Product.IsActive yok — Product.Status enum'unda ProductStatus.Active değerine sahip
    /// kayıt sayısı (net, tek değerli filtre, icat edilmedi).</summary>
    public int ActiveProducts { get; init; }

    public int TotalCategories { get; init; }
    public int TotalCollections { get; init; }

    /// <summary>Dealer.Category == DealerCategory.Dealer.</summary>
    public int DealerCount { get; init; }

    /// <summary>Dealer.Category == DealerCategory.Showroom.</summary>
    public int ShowroomCount { get; init; }

    public int TotalUsers { get; init; }
    public int ActiveUsers { get; init; }

    public int TotalFormSubmissions { get; init; }

    /// <summary>FormSubmission.IsRead == false.</summary>
    public int UnreadFormSubmissions { get; init; }

    /// <summary>FormSubmission.ProcessedAt == null — IsRead'den BAĞIMSIZ bir alan (bir başvuru
    /// okunmuş ama henüz işlenmemiş olabilir, Task 15/16'daki Details GET'in bilinçli olarak
    /// otomatik okundu-işaretlemesi bu ayrımı zaten koruyordu). Tek bir "Pending" alanına
    /// indirgenmedi — bilgi kaybı yaratırdı (Task 18 analiz kararı).</summary>
    public int UnprocessedFormSubmissions { get; init; }

    /// <summary>ADR-007'nin ertelenen kısmı — TranslationCoverageService.GetReportAsync().TotalMissing
    /// ile birebir aynı sayı (bkz. Application.Translations). Dil Yönetimi'ndeki eksik çeviri
    /// raporuna link ile birlikte gösterilir.</summary>
    public int MissingTranslationCount { get; init; }

    public IReadOnlyList<DashboardRecentProductDto> RecentProducts { get; init; } = [];
    public IReadOnlyList<DashboardRecentFormDto> RecentForms { get; init; } = [];
}
