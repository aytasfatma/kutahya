# TESTING — NG Kütahya Seramik Yönetim Paneli

> Bu dosya Task 12 (Testing Foundation) ile oluşturuldu. Bundan sonraki her modül task'ında bu
> dosyadaki standartlara uyulmalı ve "Bundan Sonraki Modüller İçin Definition of Done" bölümü
> kontrol listesi olarak kullanılmalıdır.

---

## 1. Test Türleri ve Ayrımı

Proje iki kalıcı test projesi kullanır:

```
tests/
  NGKutahyaSeramik.UnitTests/
  NGKutahyaSeramik.IntegrationTests/
```

**Unit Test** — bir servis sınıfının iş kurallarını, kendi kod yolunu (validation, upsert/delete
mantığı, dosya servisi ile etkileşim) sınar. Bu projede "unit test" **mock bağımlılıklarla saf
izolasyon** anlamına gelmez — "sociable unit test" yaklaşımı kullanılır: servis, gerçek repository
implementasyonlarıyla + SQLite in-memory DB ile test edilir (çünkü `BlogService`/`PageService`/
`ProductService` gibi servislerin "Create" akışı, entity `Id`'sinin `SaveChanges` sonrası EF
tarafından atanmasına bağımlıdır — bu davranış olmadan Moq ile anlamlı test edilemez). Yalnızca
gerçekten dış/yavaş sınır olan **dosya depolama (disk I/O)** ve **Translation persistence**
sahtelenir (`FakeFileStorageService`, `FakeTranslationService`) — DB/entity davranışı gerçek kalır.

**Integration Test** — gerçek HTTP pipeline'ı (`WebApplicationFactory<Program>`) üzerinden
Controller/routing/[Authorize]/AntiForgery/PRG/model binding'i sınar. `Program.cs` olduğu gibi
çalışır; yalnızca DbContext (SQLite in-memory) ve authentication (TestAuthHandler) test için
değiştirilir.

Kural — kavramsal karışıklık oluşturmayın:
- **Gerçek HTTP isteği (`HttpClient.GetAsync`/`PostAsync` ile bir Controller action'a ulaşmak) her
  zaman Integration test'tir.**
- **Bir servis metodunun (repository/dosya servisi enjekte edilerek) doğrudan çağrılması, HTTP
  katmanı hiç devrede olmadan, Unit test'tir.**

### Bu projede şu anda bulunmayan / gelecek faza ait olanlar
- Gerçek dış API contract testleri (proje henüz hiçbir dış API'ye bağlı değil — bkz. §9).
- UI/E2E browser testleri (Selenium/Playwright vb.).
- Load/performance testleri.
- Production ortamına karşı smoke testleri (bkz. `Migration/Index Audit` bölümü — yalnızca yerel
  "fresh database" testi yapılır, gerçek production'a asla dokunulmaz).

---

## 2. Test Projeleri ve Paketler

| Proje | Amaç | Ana Paketler |
|---|---|---|
| `NGKutahyaSeramik.UnitTests` | Servis/seeder testleri + ortak test altyapısı (Factory/Mock/Fixture) | xunit, FluentAssertions, Moq, `Microsoft.EntityFrameworkCore.Sqlite`, coverlet.collector |
| `NGKutahyaSeramik.IntegrationTests` | HTTP pipeline testleri (RBAC/AntiForgery/PRG/DB constraint) | xunit, FluentAssertions, `Microsoft.AspNetCore.Mvc.Testing`, `Microsoft.EntityFrameworkCore.Sqlite`, coverlet.collector |

`IntegrationTests`, `UnitTests` projesine referans verir (factory'lerin/fixture'ların tekrarını
önlemek için) — testler arası kod paylaşımı buradan gelir.

**Neden `UseInMemoryDatabase` değil?** EF Core'un InMemory sağlayıcısı FK/unique constraint/cascade
delete davranışlarının **hiçbirini** uygulamaz — bu tür bir davranışı sınayan bir test InMemory ile
her zaman "PASS" görünür, gerçek bir hata varsa bile yakalanmaz (yanlış-pozitif risk). Bunun yerine
**SQLite in-memory** (`Microsoft.Data.Sqlite`, `DataSource=:memory:`, bağlantı açık tutularak)
kullanılır — gerçek ilişkisel/constraint davranışı sergiler. Bkz. `SqliteTestDatabase.cs`.

**Bilinen SQLite/SQL Server farkları ve nasıl ele alındığı** (`SqliteCompatibleModelCustomizer.cs`):
- `Translation.Value` gibi `HasColumnType("nvarchar(max)")` alanları SQLite'ta sözdizimi hatası
  verir → test modelinde bu tip adı temizlenir (üretim migration/konfigürasyonu değişmez).
- Üretim SQL Server veritabanının collation'ı `SQL_Latin1_General_CP1_CI_AS` (case-insensitive,
  sqlcmd ile doğrulandı) — SQLite varsayılanı case-sensitive'dir → test modelinde tüm string
  kolonlara `NOCASE` collation uygulanır, böylece Tag.Name/ProductCode gibi case-insensitive
  tekilleştirme davranışı testte de gerçekçi şekilde sınanabilir.

---

## 3. Ortak Test Altyapısı

```
UnitTests/
  Common/
    SqliteTestDatabase.cs          — izole SQLite in-memory DbContext üretimi
    SqliteCompatibleModelCustomizer.cs
    ServiceTestContext.cs          — servis testleri için tek-durak kurulum (DB+fake'ler)
    IdentityTestHost.cs            — seeder testleri için gerçek AddIdentity+AddDbContext grafiği
    FakeUnitOfWork.cs
  Factories/
    CategoryFactory, CollectionFactory, ProductFactory, BlogFactory, BlogCategoryFactory,
    PageFactory, PageContentBlockFactory, LanguageFactory, ApplicationUserFactory,
    ImageUploadFactory (geçerli/geçersiz magic-byte imzalı görsel stream'leri)
  Mocks/
    FakeTranslationService.cs      — ITranslationService'in bellek-içi, gerçek upsert/delete sözleşmeli sahtesi
    FakeFileStorageService.cs      — IFileStorageService'in gözlemlenebilir (SaveCalls/DeleteCalls) sahtesi
  Services/                        — BlogServiceTests, PageServiceTests, PageContentBlockServiceTests, ProductServiceTests
  Seeding/                         — IdentitySeederTests, LanguageSeederTests

IntegrationTests/
  Fixtures/
    CustomWebApplicationFactory.cs — Program.cs'i SQLite+TestAuthHandler ile çalıştırır
  Authentication/
    TestAuthHandler.cs             — X-Test-Role header'ından ClaimsPrincipal üretir
    TestClientExtensions.cs        — client.AsAdmin()/.AsContentEditor()/.AsSeoEditor()/.AsProductManager()/.AsAuthenticatedWithoutRole()/.AsAnonymous()
  Security/
    AntiForgeryHelper.cs           — gerçek GET+token+POST akışı (AntiForgery hiç kapatılmaz)
  Controllers/                     — AnonymousAccessTests, RbacTests
  Database/                        — RelationalConstraintTests (SqliteTestDatabase ile doğrudan)
```

### Factory kullanımı — okunabilir, kontrollü kurulum

```csharp
var category = CategoryFactory.CreateRoot();
var product  = ProductFactory.CreateValid(productCode: "TEST0001RP", categoryId: category.Id, collectionId: collection.Id);
var draft    = BlogFactory.CreateDraft(blogCategoryId: null);
var videoBlock = PageContentBlockFactory.CreateVideoBlock(pageId, "https://youtube.com/embed/x");
var admin    = ApplicationUserFactory.CreateAdmin();
```

Factory'ler: (a) geçerli varsayılan değerler kullanır, (b) testin değiştirmek istediği alanlar
override edilebilir parametrelerdir, (c) production business logic'i **kopyalamaz** (yalnızca
entity constructor'ını çağırır), (d) entity invariant'larıyla çelişmez.

### Mock/Fake kullanımı

- **Moq**: yalnızca gerçekten "bu çağrı yapıldı mı" doğrulamasının yeterli olduğu, DB'siz basit
  repository senaryolarında (bu projede repository'ler EF pass-through olduğu için pratikte az
  kullanılır — DB-backed gerçek repository'ler tercih edilir).
  yeterli olduğu, DB'siz basit repository senaryolarında.
- **FakeTranslationService / FakeFileStorageService**: gerçek Infrastructure implementasyonlarının
  DB/disk gerektirmeyen, davranış sözleşmesi aynı kalan sahteleri. Servis testlerinde varsayılan tercih.

### Authenticated client üretimi (Integration testler)

```csharp
var admin = factory.CreateClient(new() { AllowAutoRedirect = false }).AsAdmin();
var seo   = factory.CreateClient(new() { AllowAutoRedirect = false }).AsSeoEditor();
var anon  = factory.CreateClient(new() { AllowAutoRedirect = false }).AsAnonymous();
```

Gerçek cookie/login akışı **test edilmiyor** — `TestAuthHandler`, `X-Test-Role` header'ından okuduğu
rolle bir `ClaimsPrincipal` üretir; `[Authorize(Roles=...)]` middleware'i gerçek haliyle çalışır.
Header yoksa istek tamamen anonim kalır. Bu, [Authorize] ve RBAC matrisini gerçekten sınar; yalnızca
"kim giriş yapmış" sorusunun cevabı sahte veridir. **AntiForgery bundan tamamen bağımsızdır ve hiçbir
noktada devre dışı bırakılmaz.**

### AntiForgery test yaklaşımı

```csharp
var response = await AntiForgeryHelper.PostWithAntiForgeryAsync(
    client, formUrl: "/Page/Create", postUrl: "/Page/Create", formValues: new() { ... });
```

Önce forma GET atılır (antiforgery cookie'si `WebApplicationFactory`'nin varsayılan
`HandleCookies=true` istemcisi tarafından otomatik saklanır), HTML'den `__RequestVerificationToken`
gizli alanı regex ile çıkarılır ve POST body'sine eklenir. Token'sız POST → 400 (gerçek reddedilme).

### Test veritabanı yaklaşımı

Her `SqliteTestDatabase.Create()` çağrısı **yeni, izole** bir `:memory:` bağlantısı açar — testler
arası veri sızıntısı yoktur, sıraya bağımlılık yoktur, tekrarlı çalıştırmalarda aynı sonuç garantidir.
Production veritabanına **hiçbir testte dokunulmaz**.

---

## 4. Coverage Komutu

```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
reportgenerator "-reports:TestResults/*/coverage.cobertura.xml" "-targetdir:TestResults/CoverageReport" "-reporttypes:Html;TextSummary"
```

`ReportGenerator` opsiyoneldir (`dotnet tool install -g dotnet-reportgenerator-globaltool`), HTML +
metin özet raporu üretir. `TestResults/` klasörü `.gitignore`'da — kaynak kontrolüne girmez.

Güncel sonuçlar (Task 12 sonu, bkz. kapanış raporu §16) için `TestResults/CoverageReport/Summary.txt`.

**Global coverage eşiği bu foundation task'ında dayatılmaz** (yalnızca Blog/Page/PageContentBlock/
Product servisleri + seeder'lar hedeflendi, projenin geri kalan 10+ modülü henüz test edilmedi —
bu bilinçli bir kapsam sınırlamasıdır, "yüzde yükseltmek için anlamsız test" yazılmadı). CI coverage
threshold'u ileriki bir "final stabilization" task'ında kesinleştirilecektir.

---

## 5. Dış API Mock Standardı

Proje şu anda hiçbir dış API'ye (SAP/CRM/ödeme vb.) bağlı değil — bu yüzden sahte bir dış API
sistemi **eklenmedi**. Ancak gelecekteki entegrasyonlar için standart:

- Harici servisler her zaman bir **interface** arkasında olmalı (`IFileStorageService` deseninin
  aynısı).
- **Unit testlerde** interface Moq veya elle yazılmış bir Fake ile sahtelenmeli.
- **Integration testlerde bile gerçek internet çağrısı yapılmamalı** — dış servis WireMock benzeri
  bir sahte sunucu veya aynı Fake ile karşılanmalı.
- Timeout/failure/invalid response senaryoları mutlaka mock ile test edilmeli (yalnızca "happy path"
  yetmez).
- Testler internet bağlantısına bağımlı olmamalı — CI/offline ortamda da güvenilir çalışmalı.

---

## 6. Seeder Testleri

`IdentitySeederTests` ve `LanguageSeederTests` (`UnitTests/Seeding/`) şunları doğrular:
ilk çalıştırmada oluşturma, ikinci çalıştırmada duplicate oluşturmama, eksik rollerin tamamlanması,
mevcut dilin korunması, development test kullanıcısının doğru role atanması, admin parolasının
ikinci çalıştırmada değişmemesi (checksum karşılaştırması). `IdentityTestHost` gerçek
`AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>()` grafiğini
SQLite üzerinde kurar — `RoleManager`/`UserManager` gerçek Identity store'larıyla çalışır.

---

## 7. Migration / Index Audit Standardı

Her yeni modül task'ında:
1. Migration oluşturmadan önce **clean build** alınır.
2. Oluşan migration dosyası okunur — **yalnızca beklenen tablo/kolon/index** değişikliği içermeli.
   Beklenmeyen bir DROP/ALTER varsa migration uygulanmadan durulur ve sebep raporlanır.
3. Eklenen her yeni sorgu deseni (ör. "WHERE X ORDER BY Y") için composite index ihtiyacı
   değerlendirilir — ama **"her koluna index" yaklaşımı kullanılmaz**; yalnızca gerçekten var olan
   bir sorgu deseniyle gerekçelendirilmiş index eklenir (bkz. Task 12 kapanış raporu §13-14).
4. Var olan migration dosyaları **asla** geriye dönük değiştirilmez — eksik/yanlış bulunursa yeni,
   açık isimli bir corrective migration eklenir (`AddPerformanceAndConstraintIndexes` örneği gibi).
5. "Fresh database" testi: yeni, boş bir veritabanına tüm migration zinciri baştan uygulanır,
   seeder çalıştırılır, uygulama gerçekten ayağa kalkar mı doğrulanır (mevcut geliştirme veritabanı
   asla drop edilmez — ayrı, geçici bir test veritabanı kullanılır).

---

## 7A. Büyüyen Veri Setleri İçin Pagination/Sorgu Testleri (Task 15'te eklendi)

Şimdiye kadarki modüllerin çoğu (Category, Product, Blog, News, Dealer...) `GetAllAsync()` ile tüm
kayıtları çekip Controller/DTO seviyesinde in-memory filtreliyordu — operasyonel olarak sınırlı
büyüyen veri setleri (işletme tarafından yönetilen kategori/ürün/koleksiyon sayısı) için bu yeterli
ve daha basit. **Kullanıcı gönderimleri gibi sürekli büyüyen veri setlerinde** (`FormSubmission` —
Task 15, ADR-015) bunun yerine repository'de gerçek `IQueryable` tabanlı `.Where()`/
`.OrderByDescending()`/`.Skip()`/`.Take()` + ayrı `.CountAsync()` kullanılır — hiçbir noktada tüm
tablo belleğe çekilmez.

Bu deseni test ederken:
- `SqliteTestDatabase`/`ServiceTestContext` üzerinden **gerçek repository** kullanılır (mock değil)
  — `Skip`/`Take`/`Where` ifadelerinin gerçekten doğru SQL'e çevrilip çalıştığını doğrulamanın tek
  yolu budur.
- Filtre kombinasyonları (tip, okunma durumu, tarih aralığı, arama terimi) ayrı ayrı test edilir.
- Sayfalama testi en az: sayfa başına kayıt sayısı, `TotalCount`, `TotalPages` ve iki farklı sayfanın
  **kesişmeyen** kayıtlar döndürdüğü doğrulanır.
- Sıralamanın (`CreatedAt DESC` gibi) gerçekten uygulandığı doğrulanır.
- **Yeni bir modül sürekli büyüyen bir veri seti yönetiyorsa** (log/audit kaydı, form/lead, bildirim
  kuyruğu vb.) bu pagination deseni doğrudan tekrar kullanılmalı — `GetAllAsync()` + in-memory
  filtreleme yeniden yazılmamalı.

---

## 7B. Kendi-Hesap/Kimlik Guardrail Testleri İçin Per-Test Factory Deseni (Task 16'da eklendi)

`TestAuthHandler`, "mevcut oturumdaki kullanıcı"yı sabit `X-Test-Role` claim'ine ek olarak sabit bir
`ClaimTypes.NameIdentifier = "test-user-id"` ile temsil eder. Bir modül "kendi hesabına karşı işlem
yapamama" gibi bir guardrail içeriyorsa (Task 16 — Kullanıcı Yönetimi: kendi hesabını silme/
pasifleştirme reddi), testin bu id'de **gerçek bir kayıt** oluşturması gerekir.

Diğer tüm integration test sınıfları `IClassFixture<CustomWebApplicationFactory>` ile TEK bir
factory/veritabanı örneğini sınıf genelinde paylaşır (hızlı, ama testler arası state paylaşılır).
Kendi-hesap guardrail testleri için bu **uygun değildir** — aynı sınıf içindeki birden fazla test
`"test-user-id"` id'sinde kayıt oluşturmaya çalışırsa ikinci test çakışma nedeniyle başarısız olur.

**Çözüm:** Bu tür testler `IClassFixture` KULLANMAZ; her test kendi `CustomWebApplicationFactory`
örneğini oluşturur (`await using var factory = new CustomWebApplicationFactory();`), tam izolasyon
sağlar, sıfır ekstra temizlik kodu gerektirmez. Maliyet: her test kendi host'unu ayağa kaldırdığı
için biraz daha yavaş — yalnızca gerçekten kimlik-özel state gerektiren az sayıda test için kabul
edilebilir (bkz. `UserManagementTests.cs`). Sıradan RBAC/AntiForgery testleri için paylaşılan
`IClassFixture` deseni değişmeden kullanılmaya devam eder.

---

## 8. Test Komutları (Developer Experience)

```bash
dotnet restore
dotnet build
dotnet test
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

# Migration
dotnet ef migrations add <Ad> --project src/Infrastructure/Infrastructure.csproj --startup-project src/Presentation/Presentation.csproj --output-dir Persistence/Migrations
dotnet ef database update --project src/Infrastructure/Infrastructure.csproj --startup-project src/Presentation/Presentation.csproj

# Docker (temiz başlangıç)
cp .env.example .env   # gerçek parolalarla düzenleyin
docker compose up --build
```

Docker Compose başlangıcında `DatabaseInitialization__ApplyMigrationsOnStartup=true` ile container
kendi kendine migrate+seed olur (bkz. §11 Docker bölümü, kapanış raporu).

---

## 9. Bundan Sonraki Her Modül İçin Definition of Done

Her yeni CMS modülü (veya mevcut modül değişikliği) için asgari:

- [ ] Domain/service unit testleri (yeni servis sınıfı için `ServiceTestContext` deseni)
- [ ] Kritik business rule testleri (zorunlu alan, duplicate kontrolü, ilişki validasyonu)
- [ ] Translation upsert/delete testleri (`FakeTranslationService` ile)
- [ ] Dosya yükleme varsa storage mock testleri (`FakeFileStorageService` ile — save/delete çağrı doğrulaması)
- [ ] En az bir controller integration testi (`CustomWebApplicationFactory` ile)
- [ ] Anonymous access testi (401/302/403 — en az bir GET + bir POST endpoint)
- [ ] Rol-bazlı authorization testi (en az "izinli rol → 200/302" + "izinsiz rol → 401/403")
- [ ] AntiForgery testi (token'sız POST → 400) — yeni bir Create/Edit action eklendiyse
- [ ] PRG testi (başarılı POST → 302 redirect)
- [ ] Migration review (yalnızca beklenen değişiklik var mı)
- [ ] Index review (yeni sorgu deseni varsa composite index ihtiyacı değerlendirildi mi)
- [ ] Clean build (0 Error, mümkünse 0 Warning)
- [ ] Geçici test verisi/dosyası temizliği
- [ ] Dokümantasyon güncellemesi (PROJECT_MEMORY.md / PROGRESS.md / TASKS.md)

**Geçici console verification harness'i (`--verify-X` bayrağı ile Program.cs'e eklenen, iş bitince
silinen kod) artık kalıcı test yerine KABUL EDİLMEZ.** Task 1-11 arasında bu yaklaşım kullanıldı ve
her seferinde eldeğmemiş kod olarak silindi — Task 12'den itibaren tüm modüller yukarıdaki kalıcı
test altyapısını kullanmalıdır. Gerekirse hızlı bir manuel smoke doğrulaması (`curl` ile anonim
erişim kontrolü gibi) hâlâ yapılabilir, ama otomatik testlerin **yerine geçmez**, onları tamamlar.
