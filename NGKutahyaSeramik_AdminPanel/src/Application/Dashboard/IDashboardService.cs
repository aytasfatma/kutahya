namespace Application.Dashboard;

/// <summary>
/// Madde 17.2 modül #1 (Dashboard) — dokümanda kart/grafik tanımı yok, yalnızca modül adı geçiyor
/// (Task 18 analiz kararı). Salt-okunur özet: mevcut 6 entity üzerinde sayaçlar + son 5 ürün/form.
///
/// Bu servis `AppDbContext`'e (Infrastructure.Persistence) doğrudan bağımlı olmak zorunda —
/// Application projesi Infrastructure'a referans veremiyor (ADR-016'nın genel ilkesi: Identity
/// tiplerine özgü değil, herhangi bir Infrastructure-only tipe bağımlılık aynı çözümü gerektirir).
/// Mevcut 6 modül repository'sine (`IProductRepository` vb.) yeni `CountAsync` metodu EKLENMEDİ —
/// hepsi zaten `GetAllAsync()` tam liste döndürüyor (ADR-015: işletme-yönetimli sınırlı veri
/// setlerinde bu yeterli), Dashboard'ın 6 farklı entity'ye tek seferlik salt-okunur COUNT/TOP-N
/// erişimi için bunları genişletmek yerine bu servis doğrudan DbContext kullanıyor.
/// </summary>
public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync();
}
