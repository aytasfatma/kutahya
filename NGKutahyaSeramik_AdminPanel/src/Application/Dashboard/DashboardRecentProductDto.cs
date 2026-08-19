using Domain.Enums;
using Application.Products;

namespace Application.Dashboard;

/// <summary>"Ad" alanı olarak ProductCode kullanılıyor — Translation-tabanlı çevrilebilir isim
/// (Product.DisplayName) tek entity başına ayrı bir sorgu gerektirir (ITranslationService toplu/batch
/// bir okuma metodu sağlamıyor); 5 kayıtlık bir özet widget'ı için bu ek sorgu yükü haklı görülmedi
/// (Task 18 analiz kararı — N+1'den bilinçli kaçınma). ProductCode zaten projede birincil, benzersiz,
/// insan-okunabilir tanımlayıcı olarak kullanılıyor (bkz. Product/Index.cshtml "Ürün Kodu" sütunu).</summary>
public class DashboardRecentProductDto
{
    public int Id { get; init; }
    public string ProductCode { get; init; } = string.Empty;
    public ProductStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }

    public string StatusLabel => ProductEnumDisplay.GetStatusLabel(Status);
}
