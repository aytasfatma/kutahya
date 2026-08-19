# PROGRESS — NG Kütahya Seramik Admin Panel

> Bu dosya, yeni bir sohbet açıldığında **tek başına** okunarak kaldığımız yerden hiçbir bilgi kaybı olmadan devam edebilmek için tutulur. Ayrıntılı gerekçe/ADR metinleri için `PROJECT_MEMORY.md` ve `ARCHITECTURE_DECISIONS.md`'ye bakılabilir; bu dosya onların özet/navigasyon katmanıdır.

---

# 1. Genel Proje Durumu

- **Proje adı:** NG Kütahya Seramik Admin Panel
- **Proje klasörü:** `NGKutahyaSeramik_AdminPanel`
- **Bu fazda teslim edilecek kapsam:** Yalnızca Admin Panel + Admin Panel'in kullandığı backend. Public ziyaretçi sitesi bu fazda geliştirilmiyor (mimari, ileride public site ve SAP/CRM entegrasyonu eklenebilecek şekilde kuruluyor — bkz. ADR-001, ADR-002, ADR-009).
- **Şu an bulunduğumuz faz:** Faz 2 tamamen kapandı — **Alan-seviyeli RBAC (backlog #23), Dil Yönetimi paneli (#3), SEO veri sözleşmesi+Yönetimi (#4) ve Excel Import (#17) TAMAMLANDI (21.07.2026, Task 20-23)**, doküman Madde 17.2'nin 15 panel modülünün tamamı artık kod tabanında var. Ayrıntı `PROJECT_MEMORY.md` "Bulunduğumuz Task". Kritik bulgular: Banner'ın hiç SEO alanı yok, ReferenceProject yalnızca SeoUrl'e sahip (kullanıcının "8 modül" varsayımı koda göre düzeltildi); Excel şablonunun 11 sütunu mevcut Product validasyonuyla çelişiyordu (kullanıcı onayıyla Excel-import'a özel gevşetilmiş kural seti). Solution toplamı 326 → **402/402 test**. Translation/Language altyapısı, Kategori (4.1), Koleksiyon (4.2), Ürün çekirdek CRUD (5), Ürün Görselleri (5.1), Katalog/Doküman Yönetimi (6), Referans Proje Yönetimi (7), Blog Yönetimi (8), Haber Yönetimi (9), Banner Yönetimi (10), Sayfa Yönetimi (11), Testing Foundation (12), Haber Test Sertleştirmesi (13), Bayi/Showroom Yönetimi (14), Form Yönetimi (15), Kullanıcı Yönetimi (16/16B), Role Management (17), Dashboard (18) tamamlandı; **Tabler UI Entegrasyonu — Faz 1 Prototip TAMAMLANDI (20.07.2026)** — bir iş modülü değil, yalnızca Presentation/UI katmanını kapsayan görsel yenileme (backend'e sıfır dokunuş). Tabler (MIT, github.com/tabler/tabler) v1.4.0 core + Tabler Icons v3.45.0 `wwwroot/lib/` altına **yerel** (CDN yok, sabit sürüm) eklendi; NG'ye özgü bronz/antrasit marka teması (`site.css`'te `--tblr-*` token override'ları) uygulandı; rol-bazlı (gerçek `ViewRoles` sabitlerinden) 5 gruplu sidebar + topbar kuruldu; Login için ayrı `_LoginLayout.cshtml`. Prototip kapsamı talimatın kendi sınırıyla 8 ekranla sınırlı tutuldu: `_Layout`, Sidebar, Topbar, Login, Dashboard, Product Index/Create/Edit, User Index. Diğer 9 modül henüz eski çıplak HTML'de (bilinçli, yaygınlaştırma sonraki oturuma bırakıldı). 326/326 test değişmeden geçti, route/form/AntiForgery/RBAC davranışı sıfır değişti. Ardından iki küçük takip yapıldı (20.07.2026, ayrıntı `PROJECT_MEMORY.md` "Bulunduğumuz Task" → Takip #1/#2): (1) Ürün formundaki çeviri alanları alt alta fieldset'ten Tabler Tabs'e çevrildi (7 dil, ayrı sekme, TR varsayılan aktif), veri/binding/doğrulama değişmedi; (2) seed edilen admin e-postası `admin@localhost` → `admin@localhost.com` olarak güvenli şekilde düzeltildi (User Secret güncellendi + mevcut kullanıcı `UserManager` ile yerinde yeniden adlandırıldı, duplicate admin oluşmadı, Login placeholder'ı güncellendi). Her iki takipte de 326/326 test değişmeden geçti.
- **Genel ilerleme durumu:** Kimlik doğrulama/yetkilendirme temeli %100 tamamlandı — Kullanıcı Yönetimi CRUD + Role Management denetim ekranıyla birlikte admin panelden tam yönetilebiliyor (dinamik rol CRUD hariç, dokümanda dayanağı olmadığı için hiç açılmayan bir kapsam). Dashboard artık gerçek verileri özetleyen çalışır bir ekran (önceden boştu), artık Tabler tabanlı görselle. Translation/Language altyapısı, Kategori, Koleksiyon, Ürün (çekirdek), Ürün Görselleri, Katalog/Doküman Yönetimi, Referans Proje Yönetimi, Blog Yönetimi, Haber Yönetimi, Banner Yönetimi, Sayfa Yönetimi, Bayi/Showroom Yönetimi, Form Yönetimi, Kullanıcı Yönetimi, Role Management ve Dashboard tam çalışır durumda. Application katmanı artık on üç modülün gerçek kodunu + Kullanıcı/Rol Yönetimi/Dashboard'ın arayüzlerini içeriyor. Proje artık **402 testlik** kalıcı otomatik test altyapısına sahip. Dil Yönetimi, SEO Yönetimi ve Excel Import panel ekranları artık çalışıyor (yalnızca eski çıplak HTML, henüz Tabler'a taşınmadı — bkz. aşağıdaki Tabler notu). Proje **ilk kez** bir CSS framework'e (Tabler/Bootstrap tabanlı) sahip — ama yalnızca 10 ekranda uygulandı (Category/Collection/Product/User/Dashboard/Login + iskelet), kalan modüller (Language/Seo/ProductImport dahil) eski çıplak HTML'de (tutarsızlık geçici, bilinçli). Henüz başlamayan: public form gönderim endpoint'i + e-posta bildirimi (public site fazı). Docker canlı doğrulaması bekliyor (ortam kısıtı, final stabilization task'ına bırakıldı).
- **Şu ana kadar tamamlanan büyük milestone'lar:**
  1. Kavramsal analiz ve mimari kararlar (Task 0.1, Task 0.2 — 11 ADR).
  2. Solution ve katmanlı mimari iskeleti (Task 1.1).
  3. **Identity Foundation** tamamen kapalı (Task 1.2A → 1.2B → 1.2C).
  4. **Authentication UI** tamamlandı (Task 2.1).
  5. **Role-Based Authorization** tamamlandı (Task 2.2).
  6. **Translation/Language altyapısı** tamamen kuruldu ve doğrulandı (Task 3.1A şema kararı/ADR-012 + Task 3.1B implementasyon).
  7. **Kategori Yönetimi** uçtan uca tamamlandı ve doğrulandı (Task 4.1) — Domain'in ilk CRUD entity'si, Application katmanının ilk gerçek kullanımı.
  8. **Koleksiyon Yönetimi** uçtan uca tamamlandı ve doğrulandı (Task 4.2) — ikinci CRUD entity'si, Category ile FK ilişkisi olmadığı doküman analiziyle netleşti, mevcut desenler sıfır değişiklikle tekrar kullanıldı.
  9. **Ürün Yönetimi (çekirdek CRUD)** uçtan uca tamamlandı ve doğrulandı (Task 5) — üçüncü CRUD entity'si, hem `CategoryId` hem `CollectionId`'ye bağımsız FK taşıyor; 28 native alan + 6 çevrilebilir alan; RBAC'ta bilinçli bir kapsam sınırlaması yapıldı (ayrıntı madde 2'de).
  10. **Ürün Görselleri Yönetimi** uçtan uca tamamlandı ve doğrulandı (Task 5.1) — dördüncü CRUD entity'si (`ProductImage`), projenin ilk gerçek dosya yükleme özelliği; `IFileStorageService`/`LocalFileStorageService` abstraction'ı ADR-006'yı somutlaştırdı (ADR-013).
  11. **Katalog/Doküman Yönetimi** uçtan uca tamamlandı ve doğrulandı (Task 6) — beşinci CRUD entity'si (`Document`), projenin ilk M2M ilişki modeli (`ProductDocument`/`CollectionDocument`), `IFileStorageService`'in ikinci modülde de sıfır değişiklikle tekrar kullanılabildiğini kanıtladı (ADR-014); Category/Collection silme akışındaki bir güvenlik/kararlılık açığı da bulunup düzeltildi.
  12. **Referans Proje Yönetimi** uçtan uca tamamlandı ve doğrulandı (Task 7) — altıncı CRUD entity'si (`ReferenceProject`), `Product` ile ikinci M2M ilişki modeli (`ProductReferenceProject`), kendi galeri+kapak görsel entity'si (`ReferenceProjectImage`); Madde 30 RBAC tablosunun bu modülü listelememesi Madde 7.2 analiziyle çözüldü; `IFileStorageService` üçüncü modülde de sıfır değişiklikle tekrar kullanılabildiğini kanıtladı.
  13. **Blog Yönetimi** uçtan uca tamamlandı ve doğrulandı (Task 8) — üç yeni entity (`Blog`, `BlogCategory`, `Tag` + `BlogTag` junction), `Blog`'un tekil kapak görseli ayrı bir tabloya değil doğrudan entity'ye yazıldı (ProductImage/ReferenceProjectImage'ın "galeri" gerekçesi burada yok); Madde 28.2 nedeniyle yeni `EntityType.BlogCategory` eklendi; `IFileStorageService` dördüncü modülde de sıfır değişiklikle çalıştı; Madde 30'un Blog/Haber RBAC satırı bu kez literal olarak tabloda mevcuttu (ReferenceProject'teki gibi çıkarım gerekmedi).
  14. **Haber Yönetimi** uçtan uca tamamlandı ve doğrulandı (Task 9) — iki yeni entity (`News`, `NewsCategory`), Blog'un tüm mimari desenleri (tekil kapak görseli, nullable-FK+SetNull kategori ilişkisi, Translation-gömülü SEO) birebir tekrar kullanıldı; ama Madde 22'nin Blog'dan **kasıtlı eksik bıraktığı** Excerpt/Author/Tags Haber'e hiç eklenmedi — "aynı deseni kullan" talimatı, dokümanda kanıtlanmamış alanları kopyalamak için gerekçe sayılmadı. `IFileStorageService` beşinci modülde de sıfır değişiklikle çalıştı; yeni `EntityType.NewsCategory` eklendi.
  15. **Banner Yönetimi** uçtan uca tamamlandı ve doğrulandı (Task 10) — tek yeni entity (`Banner`), hiçbir ilişkisi yok (Category/Tag/Product'tan tamamen bağımsız). Blog/News'in tekil-görsel-doğrulama deseni tekrar kullanıldı ama Blog/News'in Status enum'u/tekil PublishDate'i **kasıtlı olarak** kopyalanmadı — Madde 16.1 burada bool IsActive (2 durum) + tarih aralığı (Start/End) + manuel DisplayOrder istiyor, doküman farkı doğru yansıtıldı. `IFileStorageService` altıncı modülde de sıfır değişiklikle çalıştı; Task 3.1B'den beri rezerve olan `EntityType.Banner` ilk kez tüketildi (yeni enum üyesi eklenmedi).
  16. **Sayfa Yönetimi** uçtan uca tamamlandı ve doğrulandı (Task 11) — iki yeni entity (`Page`, `PageContentBlock`), Page'in projede **ilk kez** IsActive/Status/PublishDate/ParentId/DisplayOrder'sız kalması (doküman hiçbirini istemiyor), 5 blok tipi (TextImage/FullWidthImage/VideoEmbed/Accordion/Tab) tek `PageContentBlock` entity'sinde birleşti — Accordion/Tab için ayrı bir grup/panel alt tablosu icat edilmedi (MVP sınırlaması, dokümanda tanımlanmıyor). `IFileStorageService` yedinci modülde de sıfır değişiklikle çalıştı; Task 3.1B'den beri rezerve `EntityType.Page` ilk kez tüketildi, yeni `EntityType.PageContentBlock` eklendi (BlogCategory/NewsCategory'den sonra üçüncü enum genişletmesi). SEO Editörü'nün alan-seviyeli meta-alan düzenleme yetkisi (Madde 30) projede hiçbir modülde alan-seviyeli RBAC olmadığı için action-seviyeli salt-görüntüleme'ye indirgendi — açık teknik borç olarak kaydedildi.
  17. **Testing Foundation** tamamlandı (Task 12) — yeni bir iş modülü değil, projenin ilk kalıcı otomatik test altyapısı: 2 yeni test projesi (`NGKutahyaSeramik.UnitTests`, `NGKutahyaSeramik.IntegrationTests`), 88/88 test geçiyor (51 unit + 37 integration), SQLite in-memory ile gerçek ilişkisel davranış testi (`UseInMemoryDatabase` **kullanılmadı**), `WebApplicationFactory<Program>` ile gerçek HTTP/RBAC (5 istemci tipi)/AntiForgery/PRG testleri, model factory'ler, seeder testleri + `DatabaseInitialization:ApplyMigrationsOnStartup`/`SeedOnStartup` config-driven başlangıç politikası (ADR-004'ün açık bıraktığı sorunun çözümü), Docker altyapısı (Dockerfile/docker-compose.yml — bu makinede Docker Desktop çalışır hale getirilemediği için canlı `docker compose up` doğrulaması yapılamadı, eşdeğer manuel doğrulama yapıldı), 12 migration'ın tam denetimi + 1 corrective migration (`AddPerformanceAndConstraintIndexes`), Page↔PageContentBlock ilişkisinin gerçekten **bire-çok** olduğunun doğrulanması (kod hiç yanlış değildi, yalnızca PROGRESS.md'deki bir ifade düzeltildi), `TESTING.md` oluşturuldu.
  18. **Haber Yönetimi Test Sertleştirmesi** tamamlandı (Task 13) — News/NewsCategory (Task 9'da zaten kurulmuştu) Task 12'nin test standardına yükseltildi: 24 yeni unit + 19 yeni integration test, solution toplamı 131/131. Domain/Application/Infrastructure/Presentation'a dokunulmadı.
  19. **Bayi/Showroom Yönetimi** tamamlandı (Task 14) — gerçekten yeni bir modül (backlog #11), tek `Dealer` entity + nullable `Category` enum (ADR-008'in açık bıraktığı 3 nokta kesinleşti: kategorisiz-17-kayıt→nullable, Translation kullanılmıyor, görsel/galeri/çalışma-saatleri eklenmedi). Projenin Translation'ı hiç tüketmeyen ilk modülü, `IFileStorageService`'i hiç kullanmayan (Task 5.1'den beri) ilk CMS modülü, ve Madde 30'da yalnızca Admin'in erişimi olan ilk modül. 165/165 test (92 unit + 73 integration), `DealerService` %93.4 line coverage.
  20. **Form Yönetimi** tamamlandı (Task 15) — gerçekten yeni bir modül (backlog #16), tek `FormSubmission` entity + `FormType` discriminator (Contact/RequestInformation/SampleRequest — Madde 29'un somut alan listesiyle tanımladığı yalnızca 3 tür, dinamik form builder yok). Status enum yerine IsRead/ReadAt/ProcessedAt nullable-zaman-damgası kullanıldı (doküman somut değer vermiyor). Translation'ı hiç tüketmeyen ikinci modül (Dealer'dan sonra). Public form gönderim endpoint'i ve e-posta bildirimi ADR-001/002/009 gereği bu fazda kurulmadı. **ADR-015 eklendi — projedeki ilk gerçek SQL-seviyesi pagination/filtreleme deseni** (`GetPagedAsync`, tüm önceki modüllerin in-memory listeleme deseninden bilinçli sapma, form kayıtları sürekli büyüyen bir veri seti olduğu için). 202/202 test (116 unit + 86 integration), `FormSubmissionService` %85.2/`FormSubmissionRepository` %92.6 line coverage.
  21. **Kullanıcı Yönetimi** tamamlandı (Task 16 analiz + Task 16B implementasyon) — backlog #2'nin CRUD/RBAC kısmı kapandı. `ApplicationUser.IsActive` (yeni alan, Identity Lockout'tan bilinçli ayrı — Lockout başarısız-girişe özel süre-sınırlı bir mekanizma, kalıcı "devre dışı bırak" kararı için doğru araç değil). `IUserManagementService` — projedeki **ilk** interface'li servis (Task 17'de `IRoleManagementService` ile ikinci kez kullanıldı): Application projesi Infrastructure'a referans veremediği için (mevcut katman grafiği) arayüz Application'da, implementasyon (`UserManager<ApplicationUser>`/`RoleManager<IdentityRole>`'a bağımlı olduğu için) Infrastructure'da — Repository'lerin arayüz/implementasyon ayrımının bir "servis"e ilk uygulanışı, mimari sınırın zorunlu kıldığı istisna. Parola admin tarafından doğrudan belirleniyor (e-posta altyapısı yok — Task 15'te doğrulanmıştı). Hard-delete + kendi-hesap + son-aktif-Admin guardrail'leri **servis katmanında** (yalnızca UI'da değil). RBAC Madde 30 satırı literal: Admin=Tam, diğer 3 rol=— (Dealer'daki salt-Admin desenle aynı). 269/269 test (148 unit + 121 integration), `UserManagementService` %88.5 line coverage.
  22. **Role Management** tamamlandı (Task 17, analiz + otomatik implementasyon) — backlog #2'nin geri kalan kısmı (dinamik rol CRUD hariç, dokümanda dayanağı olmadığı için hiç kapsama alınmadı) kapandı. Sabit 4 rol için salt-okunur `RoleController` (Index: rol listesi + aktif/toplam kullanıcı sayısı; Details: rol açıklaması Madde 7.2'den + atanmış kullanıcı listesi + statik yetki matrisi) — hiçbir state-changing action yok, AntiForgery/PRG gerekmiyor. Yetki matrisi (`RoleManagementService.PermissionMatrix`) her controller'ın gerçek `ViewRoles`/`EditRoles` sabitlerinden elle çıkarıldı — dinamik/reflection-tabanlı keşif kasıtlı olarak yapılmadı; henüz controller'ı olmayan 3 modül (Dil/SEO/Excel Import) "Henüz Uygulanmadı" işaretli. `IRoleManagementService` — ADR-016 deseninin ikinci kullanımı. `ApplicationRoles.All` tek doğruluk kaynağı: DB'de fazladan/beklenmeyen bir rol olsa bile yalnızca bu 4 rol sistem rolü olarak gösteriliyor. 297/297 test (163 unit + 134 integration), `RoleManagementService`/`RoleController` %100 line coverage. Migration gerekmedi (Identity şemasına dokunulmadı).
  23. **Dashboard** tamamlandı (Task 18, analiz + otomatik implementasyon) — Madde 17.2 modül #1, dokümanda kart/grafik tanımı olmadığı için kapsam tamamen 6 entity'nin gerçek alanlarından türetildi (tahmin edilmedi). Önceden tamamen boş olan `HomeController.Index`/`Home/Index.cshtml` artık gerçek veriye bağlı: 8 özet kartı (`TotalProducts`/`ActiveProducts` [`Product.Status==Active`, `IsActive` yok], `TotalCategories`, `TotalCollections`, `DealerCount`/`ShowroomCount` [`Dealer.Category` enum ayrımı], `TotalUsers`/`ActiveUsers`, `TotalFormSubmissions`, `UnreadFormSubmissions`+`UnprocessedFormSubmissions` [`IsRead` ve `ProcessedAt` BAĞIMSIZ iki alan, tek "Pending" alanına indirgenmedi]) + Son 5 Ürün + Son 5 Form Başvurusu. `ApplicationUser`'da `CreatedAt` olmadığı için "son eklenen kullanıcılar" listesi eklenmedi (desteklenmiyor). `DashboardService` (Infrastructure) doğrudan `AppDbContext`'e bağımlı — ADR-016'nın Identity-özel olmayan genel bir uygulanışı (mevcut 6 modül repository'sine dokunulmadı, hepsi `AsNoTracking()`+DB-seviyesi `CountAsync()`/`OrderByDescending().Take(5)`). Dört rol de aynı kartları görür; yalnızca "Son Form Başvuruları" bölümü `FormSubmissionController.ViewRoles`'e (Admin+İçerik Editörü) saygıyla koşullu render ediliyor (link-güvenliği, `Product/Index.cshtml`'deki `canEdit` deseniyle aynı yaklaşım). 326/326 test (178 unit + 148 integration), Dashboard kodunun tamamı %100 line coverage. Migration/yeni paket yok — proje hâlâ hiçbir CSS framework kullanmıyor (Bootstrap kurulu değil), Dashboard da düz semantik HTML.
  24. **Tabler UI Entegrasyonu — Faz 1 Prototip** tamamlandı (20.07.2026) — yeni bir iş modülü değil, ilk görsel/UI yenilemesi; yalnızca Presentation katmanı, backend/route/RBAC/AntiForgery/migration'a sıfır dokunuş. Tabler v1.4.0 (`@tabler/core`) + Tabler Icons v3.45.0 (`@tabler/icons-webfont`), ikisi de MIT, `npm pack` ile indirilip yalnızca derlenmiş `.min.css`/`.min.js`/font dosyaları `wwwroot/lib/` altına **yerel** kopyalandı (CDN yok, sürüm sabitlendi, `pnpm`/Liquid/demo-backend/demo-veri/önizleme-uygulaması hiç alınmadı) — lisans metinleri yeni `THIRD_PARTY_NOTICES.md`'de. NG'ye özgü bronz/antrasit marka teması `wwwroot/css/site.css`'te Tabler'ın `--tblr-primary`/`--tblr-body-bg`/`--tblr-bg-surface`/`--tblr-border-color` token'larını override ederek uygulandı (Tabler'ın kendi dosyasına dokunulmadı); koyu sidebar için `.navbar-vertical[data-bs-theme="dark"]` altında ayrı, piksel-kesin bir ikinci override bloğu var. Yeni `_Sidebar.cshtml` (5 grup, her öğe ilgili controller'ın gerçek `ViewRoles` sabitinden `User.IsInRole` ile koşullu — **yalnızca UI kozmetiği, backend authorization hiç değişmedi**) + `_Topbar.cshtml` + Login'e özel sidebar'sız `_LoginLayout.cshtml` eklendi. Prototip kapsamı talimatın kendi sınırıyla 8 ekranla sınırlandı: `_Layout`, Sidebar, Topbar, Login, Dashboard, Product Index/Create/Edit, User Index — kalan 9 modül (Category/Collection/Document/ReferenceProject/Blog/News/Banner/Page/Dealer/FormSubmission/Role) bilinçli olarak eski çıplak HTML'de bırakıldı, yaygınlaştırma sonraki oturuma ertelendi. `asp-for`/`asp-validation-for`/`asp-validation-summary`/AntiForgery/model binding/route hiç değişmedi — yalnızca CSS sınıfları eklendi. 326/326 test değişmeden geçti (görsel değişiklik hiçbir test string'ini/route'unu/RBAC davranışını bozmadı). Migration yok (DB şemasına dokunulmadı). Proje **ilk kez** bir CSS framework kazandı — önceki tüm task'larda kayıtlı "hiçbir CSS framework yok" durumu bu task ile son buldu.

---

# 2. Tamamlanan İşler

## Task 0 — Analiz ve Mimari Kararlar

**Tamamlanan analizler:**
- Kavramsal Analiz dokümanı (`NG_Kutahya_Seramik_Kavramsal_Analiz_v2.pdf`, 58 sayfa) tamamen okundu ve analiz edildi (Task 0.1).
- Doküman madde 17.2 tablosu tek tek sayılarak **15 panel modülü** doğrulandı (Dashboard, Sayfa, Ürün, Koleksiyon/Kategori, Blog, Haber, Banner, Referans Proje, Katalog/Doküman, Bayi/Showroom, Form, Dil, SEO, Kullanıcı/Rol, Excel Import).
- Roller doğrulandı: Admin, İçerik Editörü, SEO Editörü, Ürün Yöneticisi (Madde 7.2/30).
- Modül/rol/veri bağımlılıkları çıkarıldı; "Dokümanda Açıkça Tanımlananlar / Teknik Çıkarımlar / Karar Bekleyen Konular" ayrımı yapıldı.
- İlk sürümdeki hatalar düzeltildi (modül sayısı, "onay akışı" ifadesinin yanlış yorumlanması, SEO sıralaması, güvenlik ayrımı) — düzeltme günlüğü `PROJECT_MEMORY.md`'de.

**Mimari kararlar (Task 0.2 — 11 ADR, hepsi onaylı):**

| ADR | Konu | Karar (özet) |
|---|---|---|
| ADR-001 | Proje kapsamı | Yalnızca Yönetim Paneli + Backend API; public site gelecek faz |
| ADR-002 | Backend API sınırı | Yalnızca panel CRUD/auth/operasyonel ihtiyaçları; public/SAP/CRM endpoint yok |
| ADR-003 | Uygulama tipi | Server-rendered ASP.NET Core MVC; SPA/Razor Pages yok |
| ADR-004 | EF Core yaklaşımı | Code First; mevcut DB yok; migration'lar CLI ile |
| ADR-005 | Authentication | ASP.NET Core Identity + Cookie; JWT yok; RBAC 4 rol |
| ADR-006 | Dosya saklama | Yerel Dosya Sistemi + storage abstraction (interface henüz yazılmadı) |
| ADR-007 | Çoklu dil veri modeli | Merkezi Translations (EntityType/EntityId); nihai şema **açık** |
| ADR-008 | Bayi/Showroom | Tek `Dealer` entity + Category alanı |
| ADR-009 | Public site entegrasyon sınırı | Ek iskelet yok; sadece katman ayrımı yeterli |
| ADR-010 | SAP entegrasyonu | Tamamen Faz 2; Faz 1'de hiçbir hazırlık yok |
| ADR-011 | Güvenlik teknik karşılıkları | Rate Limiting=yerleşik; Loglama=ILogger+Serilog; Dosya doğrulama=MIME+magic-byte+whitelist |

**Doküman incelemeleri:** Tüm ADR'ler dokümanın ilgili maddelerine (Madde 4, 17, 28, 30, 31, 35-40 vb.) atıfla gerekçelendirildi; dokümanda karar bulunmayan noktalar (fallback davranışı, dosya isimlendirme standardı, RTL kapsamı vb.) açık bırakıldı, uydurulmadı.

---

## Task 1 — Identity Foundation (TAMAMEN KAPALI)

### Solution Yapısı ve Katmanlı Mimari (Task 1.1)
- `NGKutahyaSeramik_AdminPanel.sln`, 4 proje: **Presentation** (ASP.NET Core MVC, çalıştırılabilir), **Application** (class library), **Domain** (class library, framework'ten tamamen bağımsız, hâlâ sıfır paket), **Infrastructure** (class library).
- Reference graph: Presentation→(Application, Infrastructure); Application→Domain; Infrastructure→(Application, Domain); Domain→(hiçbiri). Döngüsel bağımlılık yok.
- Tüm projeler **`net9.0`** (başlangıçta yanlışlıkla net10.0 kurulmuş, sonradan net9.0'a düşürüldü ve doğrulandı).
- `AddApplicationServices`/`AddInfrastructureServices` DI extension noktaları kuruldu.
- Serilog (Console sink) Presentation'a kuruldu.

### ASP.NET Core Identity + SQL Server (Task 1.2A)
- `Infrastructure/Identity/ApplicationUser.cs` — `IdentityUser`'dan türeyen, **hiçbir ek property yok**.
- `Infrastructure/Identity/ApplicationRoles.cs` — 4 rol **`const string`** olarak tanımlı (enum değil): `Admin`, `ContentEditor`("İçerik Editörü"), `SeoEditor`("SEO Editörü"), `ProductManager`("Ürün Yöneticisi").
- `Infrastructure/Persistence/AppDbContext.cs` — `IdentityDbContext<ApplicationUser>`, **hâlâ hiçbir domain `DbSet` yok**.
- `Infrastructure/DependencyInjection.cs` — `AddDbContext<AppDbContext>` (UseSqlServer + connection string guard clause) + `AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders()`. Identity'nin varsayılan parola/lockout ayarları **değiştirilmedi**.
- **Cookie Authentication ayarları** (`Program.cs`, `ConfigureApplicationCookie`): `LoginPath=/Account/Login`, `AccessDeniedPath=/Account/AccessDenied`, `HttpOnly=true`, `SecurePolicy=Always`, `SameSite=Lax`. `ExpireTimeSpan`/`SlidingExpiration` bilinçli olarak ayarlanmadı — Identity varsayılanları (14 gün / sliding=true) korunuyor.
- Paketler (9.0.18): Infrastructure → `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.AspNetCore.Identity.EntityFrameworkCore` + `<FrameworkReference Include="Microsoft.AspNetCore.App" />` (derleme hatası nedeniyle gerekli olduğu ampirik olarak doğrulandı). Presentation → `Microsoft.EntityFrameworkCore.Design` (`Tools` paketi yok, CLI kullanılıyor).

### InitialIdentity Migration ve Veritabanı (Task 1.2B)
- Migration adı: **`InitialIdentity`**, konumu: `Infrastructure/Persistence/Migrations/`.
- Yalnızca **7 Identity tablosu** oluşturuyor (AspNetUsers, AspNetRoles, AspNetUserRoles, AspNetUserClaims, AspNetRoleClaims, AspNetUserLogins, AspNetUserTokens) — hiçbir domain tablosu yok, kod incelenerek doğrulandı.
- Veritabanı: **Server `.\SQLEXPRESS`, Database `NGKutahyaSeramikAdminPanel`** — `dotnet ef database update` ile CLI üzerinden oluşturuldu (`Database.MigrateAsync()` kodda **kullanılmıyor**, bilinçli bir ADR-004 kararı).
- Design-time DbContext doğrulaması global `dotnet-ef 10.0.9` ile sorunsuz çalıştı — repo-yerel `9.0.18` aracı kurulumu gerekmedi.

### IdentitySeeder ve Admin Bootstrap (Task 1.2B/1.2C)
- `Infrastructure/Identity/IdentitySeeder.cs`:
  - `SeedIdentityDataAsync` — 4 rolü koşulsuz, idempotent seed eder (`RoleExistsAsync`/`CreateAsync`, `ApplicationRoles` sabitleriyle) + admin kullanıcıyı seed eder (`SeedAdmin:Email`/`SeedAdmin:Password` config'ten, eksikse WARNING+skip).
  - `SeedDevelopmentTestUserAsync` — (Task 2.2'de eklendi, aşağıya bakınız).
- **Admin bootstrap:** Email/UserName = `admin@localhost`, rol = `Admin`, `EmailConfirmed = false` (framework varsayımı, değiştirilmedi). Gerçek parola **yalnızca User Secrets'ta**.
- Doğrulamalar: tek kullanıcı, tek rol ataması, ikinci çalıştırmada duplicate yok, `PasswordHash`/`SecurityStamp`/`ConcurrencyStamp` checksum karşılaştırmasıyla parolanın overwrite edilmediği kanıtlandı.

### User Secrets Kullanımı
- Presentation projesinde `UserSecretsId` tanımlı (`dotnet user-secrets init` ile).
- Kullanılan anahtarlar (isimler — **değerler hiçbir dosyada/logda/raporda yok**):
  - `SeedAdmin:Email`, `SeedAdmin:Password`
  - `SeedTestUser:Email`, `SeedTestUser:Password`
- `appsettings.json` → `ConnectionStrings:DefaultConnection` boş placeholder. `appsettings.Development.json` → gerçek `.\SQLEXPRESS` bağlantı dizesi (Trusted Connection, kimlik bilgisi içermiyor — secret sayılmıyor).

### Build Durumu
- Task 1 boyunca her adımda `dotnet build` → **0 Warning / 0 Error**.

---

## Task 2 — Authentication & Authorization (TAMAMLANDI)

### Task 2.1 — Authentication UI
- **Login (GET/POST)** — `AccountController`: e-posta ile giriş (`FindByEmailAsync` + `PasswordSignInAsync`), `isPersistent:false`, `lockoutOnFailure:true`, RememberMe yok.
- **Logout (POST)** — yalnızca POST, `[Authorize]`, `[ValidateAntiForgeryToken]` (GET denemesi → 405, doğrulandı).
- **AccessDenied (GET)** — `[AllowAnonymous]`, statik view.
- **Dashboard** — `HomeController.Index()`, sınıf seviyesinde `[Authorize]`, kullanıcının e-postasını gösterir.
- **ReturnUrl koruması:** `Url.IsLocalUrl(returnUrl)` ile doğrulanıyor; ReturnUrl form'un **query string**'inde (`asp-route-returnUrl`) taşınıyor, hidden field değil.
- **Open Redirect koruması:** Harici URL (`https://evil.com`) denemesi gerçek form mekanizmasıyla test edildi — Home/Index'e düştü, dış siteye yönlendirilmedi (doğrulandı).
- **Kritik düzeltme:** `Program.cs` pipeline'ında `app.UseAuthentication()`/`app.UseAuthorization()` **hiç eklenmemişti** (Task 1.2A'dan kalan eksiklik — servisler kayıtlıydı, middleware yoktu). `UseRouting()` sonrası, `MapControllerRoute` öncesine eklendi; bu olmadan `[Authorize]` çalışmıyordu.
- Genel hata mesajı (enumeration koruması): **"Geçersiz e-posta veya parola."** — hem yanlış parola hem olmayan kullanıcı için aynı mesaj.
- `EmailConfirmed=false` olan admin girebiliyor (RequireConfirmedEmail hiç set edilmedi).
- 11/11 test senaryosu geçti (aşağıda madde 6'da tam liste).

### Task 2.2 — Role-Based Authorization
- **Authorization yaklaşımı:** Doğrudan `[Authorize(Roles = ApplicationRoles.Admin)]` — named policy **kullanılmadı** (ADR-005'in "gereksiz policy oluşturulmayacak" ilkesi).
- **AdminOnly action:** `HomeController.AdminOnly()`, `[Authorize(Roles = ApplicationRoles.Admin)]` ile korumalı, `Views/Home/AdminOnly.cshtml` — minimal, iş mantığı yok.
- **Development test kullanıcısı:** Email/UserName = `editor@localhost`, rol = `ApplicationRoles.ContentEditor` (İçerik Editörü). `IdentitySeeder.SeedDevelopmentTestUserAsync` — admin seed deseniyle birebir (idempotent, `SeedTestUser:Email`/`Password` config'ten, eksikse WARNING+skip, mevcut parolayı/diğer rolleri değiştirmez, `EmailConfirmed` otomatik `true` yapılmaz).
- **Environment kontrolü Infrastructure'a taşınmadı** — `Program.cs`'te yalnızca `if (app.Environment.IsDevelopment()) { await scope.ServiceProvider.SeedDevelopmentTestUserAsync(); }`.
- **RBAC doğrulamaları:** Admin → AdminOnly: 200 OK. İçerik Editörü → Home/Index: 200 OK. İçerik Editörü → AdminOnly: **organik** AccessDenied yönlendirmesi (gerçek yetkisiz erişimle tetiklendi, geçici test controller'ı kullanılmadı).
- **Production test sonucu:** `ASPNETCORE_ENVIRONMENT=Production` + `--no-launch-profile` + yalnızca ortam değişkeniyle geçici connection string kullanılarak simülasyon yapıldı — development test kullanıcı seed'inin **hiç çağrılmadığı** log'da ampirik olarak kanıtlandı (hiçbir `SeedTestUser`/`editor@localhost` izi yok).
- **Yapılan tüm test senaryoları (Task 2.1 + 2.2 birleşik, hepsi ✅):**
  1. Anonim `/Home/Index` → Login + ReturnUrl
  2. Anonim `/Home/AdminOnly` → Login + ReturnUrl
  3. Doğru admin bilgileriyle giriş → başarılı
  4. Yanlış parola → genel hata mesajı
  5. Kayıtlı olmayan e-posta → aynı genel hata mesajı
  6. Harici ReturnUrl → open redirect oluşmadı
  7. Login olmuş kullanıcı → Login GET'te Home'a otomatik yönlendirme
  8. Logout POST → cookie geçersiz
  9. Logout GET → 405 Method Not Allowed
  10. AccessDenied ekranı (doğrudan URL + organik yönlendirme, ikisi de) render edildi
  11. Admin → AdminOnly: 200 OK
  12. İçerik Editörü → Home/Index: 200 OK
  13. İçerik Editörü → AdminOnly: AccessDenied'a organik yönlendirme
  14. DB: 2 kullanıcı, 4 rol, 2 rol ataması — doğru
  15. İkinci çalıştırmada duplicate kullanıcı/rol yok (0 INSERT)
  16. Parolalar overwrite edilmedi (checksum karşılaştırması)
  17. Production simülasyonunda test user seed çağrılmadı
  18. `dotnet build` → 0 Warning / 0 Error (her task sonunda tekrarlandı)
- **Rol-cookie davranışı (belgelendi, çözülmedi):** Identity role claim'leri login sırasında cookie'ye eklenir; DB'deki sonraki rol değişiklikleri mevcut cookie'ye anında yansımaz, güncel roller için yeniden authentication (logout+login) gerekir. Manuel SQL rol değiştirme testi **bilinçli olarak yapılmadı** (kullanıcı kararı — gereksiz risk).

---

## Task 3.1A — Translation (Çoklu Dil) Şema Kararları ve Mimari Analizi (TAMAMLANDI, 19.07.2026)

- **Yalnızca analiz/karar — hiçbir entity, DbContext değişikliği, EF configuration, migration veya seeder oluşturulmadı.** ADR-007'nin ilke olarak onayladığı merkezi Translations yaklaşımının bıraktığı 6 açık şema detayı + ayrı bir `Language` entity'si kesinleştirildi.
- **Kesinleşen kararlar:** `FieldName` düz `nvarchar` (DB constraint yok, kod tarafında const string); `Value` tek `nvarchar(max)`; her (EntityType, EntityId, LanguageId, FieldName) ayrı satır (JSON yok); `EntityType` enum + Infrastructure'da açık `ValueConverter<EntityType,string>` (`.ToString()`'e bağımlı değil, eksik mapping'de exception); unique index `(EntityType, EntityId, LanguageId, FieldName)`; yetim çeviri temizliği trigger/job değil, Application katmanı (aynı transaction); `Language` entity'si (`Id`,`Code`,`Name`,`IsActive`,`DisplayOrder`), `Translation.LanguageId`→`Language.Id` **surrogate FK** (doğal anahtar/Code FK değil), `Language.Code` ayrı unique index, `Translation`'da `LanguageCode` sütunu yok, `ON DELETE RESTRICT` (dil silinmez, pasif yapılır).
- **Migration kararı:** Language + Translation tek migration'da birlikte (Task 3.1B'de); dil verisi migration'a gömülmeyecek, `IdentitySeeder` deseniyle ayrı seeder ile eklenecek.
- **ADR-012** olarak ARCHITECTURE_DECISIONS.md'ye eklendi — ADR-007 değiştirilmedi/geçersiz kılınmadı, yalnızca detaylandırıldı.
- Backlog madde #2 ("Kullanıcı/Rol Yönetimi") "RBAC altyapısı tamamlandı, Kullanıcı/Rol CRUD yönetim ekranları ileride yapılacak" ifadesiyle tamamlandı işaretlendi (TASKS.md).
- Karar süreci: FieldName/Value/satır yapısı/unique index/yetim temizliği ilk turda onaylandı; EntityType (const string mi enum mu) ve Language ilişkisi (Code FK mi Id FK mi) için ayrı derin teknik karşılaştırma turları yapıldı — kullanıcı enum+açık converter ve Id-FK yönünde karar verdi, gerekçeler ADR-012'de kayıtlı.
- **Bilinçli olarak yapılmayanlar:** Domain entity, Infrastructure configuration, `AppDbContext` değişikliği, migration, `dotnet ef database update`, seeder — hepsi Task 3.1B'ye bırakıldı.

---

## Task 3.1B — Translation & Language Infrastructure Implementation (TAMAMLANDI, 19.07.2026)

- Task 3.1A'da (ADR-012) kesinleşen kararların koda geçirilmesi. İmplementasyon öncesi 3 karar son kez yeniden değerlendirilip uygulandı: `Language.Code` normalizasyonu entity'den çıkarıldı (Application'a bırakıldı, Identity'nin `ILookupNormalizer` deseniyle tutarlı); `Language.Translations` navigation collection eklendi; `Translation.EntityId` tipi `int` sabitlendi (ileride tüm Domain entity'leri için emsal). Audit alanları (CreatedAt/UpdatedAt) ADR-004 gereği eklenmedi.
- **Oluşturulan dosyalar:** `Domain/Enums/EntityType.cs` (9 üye), `Domain/Entities/Language.cs`, `Domain/Entities/Translation.cs`, `Infrastructure/Persistence/Conversions/EntityTypeMapping.cs` + `EntityTypeConverter.cs`, `Infrastructure/Persistence/Configurations/LanguageConfiguration.cs` + `TranslationConfiguration.cs`, `Infrastructure/Persistence/LanguageSeeder.cs`.
- **Değiştirilen dosyalar:** `AppDbContext.cs` (ilk `OnModelCreating` override + 2 `DbSet`), `Program.cs` (`SeedLanguagesAsync()` çağrısı).
- **Migration:** `AddTranslationInfrastructure` — `Languages`+`Translations` tek migration'da, `InitialIdentity`'ye dokunulmadı.
- **Doğrulama (11/11 ✅):** Tablolar oluştu; 7 dil doğru sırada (TR→RU) seed edildi; 3 ayrı `dotnet run` ile 0 duplicate; `Language.Code` ve `Translation` composite unique index'leri manuel SQL ile duplicate reddi doğrulandı; geçersiz `LanguageId` FK tarafından reddedildi; `ON DELETE RESTRICT` çalıştı (RU dili silme denemesi reddedildi, **hiç silinmedi**); converter round-trip doğrulandı (`PRODUCT`→enum doğru okundu, `UNKNOWN_XYZ`→beklenen `InvalidOperationException`); build 0/0.
- Geçici doğrulama kodu (`Program.cs`'e eklenen `--verify-translations` bloğu) ve manuel SQL ile eklenen 2 geçici test satırı, doğrulama sonrası **tamamen kaldırıldı/silindi**.
- Backlog madde #3 ("Dil Yönetimi altyapısı") bu task ile **tamamen tamamlandı** işaretlendi.
- **Bilinçli olarak yapılmayanlar:** Orphan-cleanup abstraction'ı/servisi (Task 4.1'de tasarlandı), Language CRUD/Application servisi, `Code` normalizer, SEO'nun `EntityType` paylaşımı kararı, ikincil `(EntityType,EntityId)` index'i, audit alanları, herhangi bir Product/Category/vb. entity (Category Task 4.1'de eklendi).

---

## Task 4.1 — Kategori Yönetimi, uçtan uca (TAMAMLANDI, 18-19.07.2026)

**Çalışma düzeni değişikliği:** Bu task'tan itibaren plan+implementasyon+migration+database update+doğrulama+dokümantasyon tek task içinde birlikte yürütülüyor. Yalnızca dokümanla/ADR ile çelişen veya katman bağımlılığını değiştirecek kritik kararlarda durulup onay isteniyor.

- **Domain:** `Category` (`Id`, `ParentCategoryId` self-ref, `ParentCategory`/`Children`, `ImagePath`, `DisplayOrder`, `IsActive`) — sade POCO, çevrilebilir native sütun yok (Name/Description/SeoUrl/MetaTitle/MetaDescription tamamen `Translation`'da, `EntityType.Category`). `Translation.UpdateValue()` eklendi (Task 3.1B'de yoktu, ilk gerçek upsert ihtiyacı bu task'ta doğdu).
- **Application (ilk gerçek kullanımı):** `ICategoryRepository`/`CategoryService`/`CategoryDto`/`CategoryRequests`/`CategoryOperationResult`/`CategoryFields` (`Application/Categories/`), `ITranslationService` (`Application/Translations/`), `IUnitOfWork`. Application EF Core'a hiç referans vermiyor; `CategoryService` interface'siz somut sınıf.
- **Infrastructure:** `CategoryConfiguration` (self-ref FK `Restrict`), `CategoryRepository`, `TranslationService`, `UnitOfWork`; `AppDbContext`'e `DbSet<Category>`; DI (hepsi scoped).
- **Presentation:** `CategoryController` (Index/Create/Edit/ToggleActive/Delete), ağaç görünümlü liste, `_CategoryForm.cshtml` paylaşımlı form partial'ı, `_Layout.cshtml`'e nav linki.
- **Migration:** `AddCategories` — yalnızca `Categories` tablosu, self-ref FK `Restrict`, `InitialIdentity`/`AddTranslationInfrastructure`'a dokunulmadı.
- **RBAC:** Admin+Ürün Yöneticisi+İçerik Editörü+SEO Editörü görüntüleme (controller seviyesi); Admin+Ürün Yöneticisi düzenleme, yalnızca Admin silme (action seviyesi) — doğrudan `[Authorize(Roles=...)]`.
- **Doğrulama (10/10 iş kuralı ✅ + DB/RBAC kontrolleri):** Ana/alt kategori oluşturma, 3. seviye reddi, self-parent reddi, aynı-parent-aynı-ad reddi, farklı-parent-aynı-ad kabulü, edit+upsert+opsiyonel-alan-silme, toggle active, alt-kategorili-silme reddi, leaf silme+Translation temizliği — geçici `--verify-categories` kodu ile uçtan uca test edildi, tamamı beklenen sonucu verdi. sqlcmd ile şema doğrulandı ve test verisi temizliği (0 kalıntı) teyit edildi. Anonim erişim (`/Category`, `/Category/Delete`) Login'e yönlendirme ile doğrulandı. **Rol-bazlı çoklu-kullanıcı canlı testi bu oturumda yapılamadı** (gerçek parolalar User Secrets'ta, paylaşılmadı) — mekanizma Task 2.1/2.2'de kanıtlanmış aynı `[Authorize(Roles=...)]` deseni olduğu için kod incelemesiyle doğrulandı.
- Geçici doğrulama kodu doğrulama sonrası tamamen kaldırıldı.
- **Küçük, onay istenmeden alınan 2 ek karar:** `IUnitOfWork` (Application EF Core'suz + tek `SaveChangesAsync` kısıtlarını birlikte karşılamak için zorunlu); `ITranslationService.DeleteTranslationFieldAsync` (boşaltılan opsiyonel alanın silinmesi kuralı için gerekli).
- Backlog madde #5 ("Kategori Yönetimi") **tamamen tamamlandı** işaretlendi.
- **Bilinçli olarak yapılmayanlar:** Gerçek dosya yükleme/storage abstraction (ADR-006), arama/filtreleme/sayfalama/toplu işlem/Excel aktarımı, ayrı Details ekranı, drag-drop sıralama, otomatik test projesi, `CategorySeeder`, ~~Product/Collection ilişkisi~~ (Collection Task 4.2'de eklendi).

---

## Task 4.2 — Koleksiyon Yönetimi, uçtan uca (TAMAMLANDI, 19.07.2026)

**Kritik ilişki kararı — Collection ↔ Category (doküman analiziyle çözüldü, onay istenmedi):** Task 4.1A'da işaretlenen olası çelişki yeniden incelendi. 4 bağımsız kanıt **Collection'ın Category'ye FK ile bağlı olmadığını** gösterdi:
1. Madde 20 (Koleksiyon Yönetimi) Category'yi hiç anmıyor.
2. Madde 36.1 (Veri Modeli, Ana Tablolar) `Collections` satırının ilişkileri yalnızca "Products, Documents" — Categories yok.
3. Madde 27.2 (SEO URL) koleksiyon URL'i düz/bağımsız (`/koleksiyonlar/{seo-url}`), kategori segmenti yok.
4. Madde 18.1 (Ürün Veri Modeli) `Category` ve `SeriesName`/`CollectionName`'i bağımsız kardeş alanlar olarak modelliyor.

Madde 16.4'teki "Marka > Kategori > Alt Kategori > Koleksiyon > Ürün" ifadesi doküman **Bölüm 16'da** (public site modülleri) geçiyor — admin panel şeması değil, kavramsal gezinme hiyerarşisi. **Sonuç: `Collection` entity'sinde `CategoryId` yok.**

- **Domain:** `Collection` (`Id`, `ImagePath`, `DisplayOrder`, `IsActive`) — Category'den daha basit, FK/hiyerarşi yok.
- **Application:** `ICollectionRepository`/`CollectionService`/`CollectionDto`/`CollectionRequests`/`CollectionOperationResult`/`CollectionFields` (`Application/Collections/`) — Category'nin deseniyle birebir, parent mantığı yok. Mevcut `ITranslationService`/`IUnitOfWork` **değiştirilmeden tekrar kullanıldı**.
- **Infrastructure:** `CollectionConfiguration` (FK yok, gereksiz index yok), `CollectionRepository`; `AppDbContext`'e `DbSet<Collection>`; DI.
- **Presentation:** `CollectionController` (Index/Create/Edit/ToggleActive/Delete, Category dropdown'u yok), düz liste (tree değil), `_CollectionForm.cshtml`, `_Layout.cshtml`'e nav linki.
- **Migration:** `AddCollections` — yalnızca `Collections` tablosu, hiçbir FK yok; Identity/Languages/Translations/Categories'e dokunulmadı.
- **RBAC:** Category ile birebir aynı matris.
- **İş kuralı farkı:** Duplicate TR isim kontrolü Category'de "aynı parent altında" iken, Collection'da **global** — Category ilişkisi olmadığı ve doküman "272 **benzersiz** seri adı" dediği için.
- **Doğrulama (6/6 iş kuralı ✅ + DB/RBAC kontrolleri):** Oluşturma, TR adı zorunluluğu, global duplicate ad reddi, edit+upsert+opsiyonel-alan-silme, toggle active, silme+Translation temizliği — geçici `--verify-collections` kodu ile test edildi, tamamı beklenen sonucu verdi. sqlcmd ile şema + temizlik (0 kalıntı) + diğer tabloların (`Categories`=0, `Languages`=7, `Translations`=0, Identity değişmedi) etkilenmediği doğrulandı. Anonim erişim (`/Collection`, `/Collection/Create`, `/Collection/Delete`) Login'e yönlendirme ile doğrulandı. **Rol-bazlı çoklu-kullanıcı canlı testi yine yapılamadı** (aynı credential kısıtı).
- Geçici doğrulama kodu doğrulama sonrası tamamen kaldırıldı.
- **Yeni genel-amaçlı abstraction eklenmedi** — `ITranslationService`/`IUnitOfWork` hiç değiştirilmeden tekrar kullanıldı; `CategoryService`'e spekülatif Collection kontrolü eklenmedi.
- Backlog madde #6 ("Koleksiyon Yönetimi") **tamamen tamamlandı** işaretlendi.
- **Bilinçli olarak yapılmayanlar:** Product/Document ilişkisi (o entity'ler henüz yok), gerçek dosya yükleme, arama/filtreleme/sayfalama/toplu işlem, ayrı Details ekranı, drag-drop sıralama, otomatik test projesi, `CollectionSeeder` (272 gerçek seri adı — veri aktarımı task'ının konusu).

---

## Task 5 — Ürün Yönetimi, çekirdek CRUD (TAMAMLANDI, 19.07.2026)

**Kapsam sınırlaması (bilinçli, backlog'la tutarlı):** Madde 16.4/17.2, Ürün Yönetimi'ni projenin en kapsamlı modülü olarak tanımlıyor (görseller, dokümanlar, ilgili ürünler/projeler dahil). TASKS.md backlog'u bu genişliği zaten ayrı maddelere bölmüştü: #7 Ürün Yönetimi (bu task — yalnızca çekirdek `Product` entity'si), #8 Ürün Görselleri, #9 Katalog/Doküman Yönetimi, #10 Referans Proje Yönetimi. Gerçek dosya yükleme ve Document/Project many-to-many ilişkileri bu task'ta **yok**.

**Kritik ilişki kararı — SeriesName ↔ Collection (doküman analiziyle çözüldü, onay istenmedi):** Madde 18.1'in veri modeli tablosu hem `SeriesName` (Zorunlu, "Örn: AMAZONIT, ATLANTIS") hem ayrı bir `CollectionName` (Opsiyonel) serbest metin alanı listeliyor. Madde 20 ("Her seri bir koleksiyon olarak değerlendirilebilir", 272 benzersiz seri adı = Task 4.2'nin 272 potansiyel `Collection` kaydıyla birebir örtüşüyor) esas alınarak **`SeriesName`, `Product.CollectionId` FK'sine bağlandı** (zorunlu); doküman'daki ayrı `CollectionName` metin alanı eklenmedi (Collection ilişkisiyle anlamsal çakışma, doğrulanamaz ikinci alan, YAGNI). `Category` alanı da `Product.CategoryId` FK'sine bağlandı (zorunlu). Önceki oturumun bıraktığı not ("Product hem CategoryId hem CollectionId FK'sini bağımsız kardeş alan olarak taşıyacak") ile birebir tutarlı.

- **Domain:** `Product` (`Domain/Entities/Product.cs`) — 28 native alan: `ProductCode`(unique), `CategoryId`/`Category`, `CollectionId`/`Collection`, `Brand`(enum), `Status`(enum: Active/Inactive/InProgress/Cancelled), `Size`, `Unit`, `Surface`, `Relief`, `SpecialSurface`, `FaceCount`, `Thickness`, `BodyType`, `Color`, `ColorMaterial`, `ApplicationArea`, `UsageArea`, `Finish`, `PEI`, `VValue`, `RValue`, `DeepAbrasion`, `HeatResistance`, `AntiSlip`, `GlazedGranite`, `BoxM2`, `PalletM2`, `DisplayOrder`, `CreatedAt`, `UpdatedAt`. `CreatedAt`/`UpdatedAt` — Category/Collection'da yoktu, ama Madde 18.1 Product için özel olarak bunu istiyor (doküman-gerekçeli istisna). Çevrilebilir 6 alan (Name/ShortDescription/LongDescription/SeoUrl/MetaTitle/MetaDescription) `Translation`'da (`EntityType.Product`).
- **Application:** `Application/Products/` (`IProductRepository`, `ProductService`, `ProductDto`, `ProductRequests`, `ProductOperationResult`, `ProductFields`, `ProductEnumDisplay`) — mevcut `ICategoryRepository`/`ICollectionRepository`/`ITranslationService`/`IUnitOfWork` **hiç değiştirilmeden** tekrar kullanıldı.
- **Infrastructure:** `ProductConfiguration` (ProductCode unique index; CategoryId/CollectionId FK `Restrict`; Brand/Status `HasConversion<string>()` — EntityType'ın aksine bu enum'lar Product'a özgü/polimorfik olmadığı için ADR-012'deki gibi ayrı bir `ValueConverter` sınıfı gerekmedi; decimal alanlar `decimal(10,3)`), `ProductRepository` (+ `GetByProductCodeAsync`).
- **Presentation:** `ProductController` (Index/Create/Edit/Delete — Category/Collection'daki `ToggleActive` deseni yok, çünkü Product `IsActive bool` değil 4 değerli `Status` enum kullanıyor; durum değişikliği normal Edit formundan yapılıyor), Kategori (2-seviyeli ağaç flatten, "— " girinti)/Koleksiyon/Marka/Durum dropdown'ları.
- **RBAC — bilinçli kapsam sınırlaması (kritik değil, kayıt altına alındı):** Madde 30, Ürün Yönetimi için **alan-seviyeli** kısmi yetki istiyor (İçerik Editörü: yalnızca "Açıklama/Görsel"; SEO Editörü: yalnızca "Meta Alanları"). Bu, projede hiç kullanılmamış bir granülerlik (action-seviyesi yerine alan-bazlı kısmi yazma izni) gerektiriyor ve yanlış uygulanırsa güvenlik riski taşıyabilir (örn. formda gizlenen alanların POST body'sine eklenip sunucu tarafında maskelenmeden kabul edilmesi). Bu task'ta **Category/Collection ile birebir aynı action-seviyeli RBAC deseni** kullanıldı: Admin+Ürün Yöneticisi = tam CRUD, İçerik Editörü+SEO Editörü = yalnızca görüntüleme, Admin = tek başına silme. Madde 30'daki alan-seviyeli kısmi yetkiler **uygulanmadı** — modülün birincil kullanıcı rolleri (Admin, Ürün Yöneticisi) günden itibaren tam çalışıyor; kısmi düzenleme yetkisi ayrı bir backlog maddesi olarak bırakıldı.
- **Migration:** `AddProducts` — yalnızca `Products` tablosu; `InitialIdentity`/`AddTranslationInfrastructure`/`AddCategories`/`AddCollections`'a dokunulmadı.
- **Doğrulama (20/20 iş kuralı ✅ + DB/RBAC kontrolleri):** Geçerli oluşturma, TR ad/kısa açıklama zorunluluğu, duplicate ürün kodu reddi, var olmayan kategori/koleksiyon reddi, kalınlık≤0 reddi, zorunlu string alan boşluğu reddi (Size), upsert+opsiyonel-alan-silme (Relief eklenip boşaltıldı), Brand/Status güncelleme, silme+Translation temizliği — geçici doğrulama koduyla test edildi, tamamı beklenen sonucu verdi. sqlcmd ile şema + tam temizlik (Products/Categories/Collections/Translations tümü 0 satır; Identity tabloları etkilenmedi — 2 kullanıcı/4 rol aynı) doğrulandı. Anonim erişim (`/Product`, `/Product/Create`, `/Product/Delete/1`) Login'e yönlendirme ile doğrulandı. **Rol-bazlı çoklu-kullanıcı canlı testi yine yapılamadı** (aynı credential kısıtı).
- Geçici doğrulama kodu (`ProductVerification.cs` + `Program.cs`'teki `--verify-products` bloğu) doğrulama sonrası tamamen kaldırıldı.
- Backlog madde #7 ("Ürün Yönetimi") **kısmi tamamlandı** işaretlendi — çekirdek CRUD bitti, görsel/doküman/proje ilişkileri #8/#9/#10'un konusu.
- **Bilinçli olarak yapılmayanlar:** Gerçek dosya yükleme/storage abstraction (ADR-006), Document/Project many-to-many ilişkileri (o entity'ler henüz yok), Excel import, arama/filtreleme/sayfalama/toplu işlem, ayrı Details ekranı, drag-drop sıralama, otomatik test projesi, alan-seviyeli RBAC (yukarıda gerekçeli).

---

## Task 5.1 — Ürün Görselleri Yönetimi (TAMAMLANDI, 19.07.2026)

**Kritik bulgu (migration riskini baştan ortadan kaldırdı):** `Product` entity'sinde (Task 5) hiç `ImagePath` alanı yoktu — Madde 18.1'in Product veri modeli tablosu böyle bir alan içermiyor, görseller baştan Madde 18.2'nin ayrı çoklu-görsel modeline bırakılmıştı. "Mevcut alanı koru mu/kaldır mı" ikilemi hiç oluşmadı; `ProductImage` sıfırdan eklendi.

**Gereksinim incelemesi özeti:**
- **Kesin gereksinim (Madde 18.2):** 5 görsel tipi (Render/Face/Lifestyle/Texture/Detail) → `ImageType` enum. Görsel sıralaması (Ek-3 dosya isimlendirme standardının `_siraNo` kısmı) → `DisplayOrder`. Ürün silindiğinde temizlik → kullanıcı talimatı + mantıksal zorunluluk.
- **Belirsiz/MVP kararı:** Tek ana görsel (`IsPrimary`) — doküman açık istemiyor, Madde 15.3 hero-görsel vurgusu + admin thumbnail ihtiyacı için eklendi. Format (jpg/jpeg/png/webp) + 5MB boyut sınırı — doküman sınır vermiyor, gerekçeli MVP kararı.
- **Dokümanda yok, eklenmedi:** Alt metin/başlık, çoklu dil metadata, video desteği, görsel sayısı sınırı.

**Storage kararı — ADR-013 olarak kaydedildi:** `Application/Storage/IFileStorageService` (Stream-tabanlı, `IFormFile`'a bağımlı değil — Application/Domain hâlâ ASP.NET Core'dan bağımsız) + `Infrastructure/Storage/LocalFileStorageService` (`wwwroot/uploads`, path traversal koruması hem kayıt hem silme yönünde). Klasör yapısı Madde 35.4/ADR-006'ya **literal sadakatle**: `/uploads/products/{ProductCode}/{gorselTipi}/{guid}.{uzanti}` — `ProductId` değil `ProductCode` kullanıldı (ADR'ye uygunluk); `ProductCode` sonradan değişirse eski görseller eski klasör adını korur ama veri kaybı/kırık link **oluşmaz** (DB'de tam yol saklı, yeniden hesaplanmıyor) — kabul edilen kozmetik risk.

- **Domain:** `ProductImage` (`Id`, `ProductId`/`Product`, `ImageType`, `FilePath`, `IsPrimary`, `DisplayOrder`). `Product` entity'sine **hiçbir değişiklik yapılmadı** (geri-navigasyon bile eklenmedi).
- **Application:** `Application/ProductImages/` (`IProductImageRepository`, `ProductImageService`, `ProductImageDto`, `ProductImageOperationResult`, `AddProductImageRequest`, `ProductImageEnumDisplay`). `ProductImageService`: boyut/uzantı/MIME/magic-byte doğrulama (3. parti kütüphane yok, saf BCL byte karşılaştırması — JPEG `FF D8 FF`, PNG 8-bayt imza, WEBP `RIFF...WEBP`), ana-görsel otomasyonu (ilk yükleme→primary, silme→fallback, SetPrimary→eskisini otomatik kaldırır), sahiplik kontrolü (imageId/productId eşleşmezse reddedilir), başarısız DB yazımında fiziksel dosya geri alma (compensating action), `ILogger<ProductImageService>` ile hata loglama (Application katmanında ilk `ILogger<T>` kullanımı — `Microsoft.Extensions.Logging.Abstractions` paketi eklendi). `ProductService`'e `IProductImageRepository` (liste thumbnail'i için) + `ProductImageService` (silme kaskadı için) enjekte edildi.
- **Infrastructure:** `LocalFileStorageService`, `ProductImageConfiguration` (FK `Cascade`, `ImageType` `HasConversion<string>()`, **filtered unique index** `WHERE [IsPrimary]=1` — projenin ilk filtered unique index'i), `ProductImageRepository`.
- **Presentation:** Ayrı `ProductImageController` (Upload/SetPrimary/MoveUp/MoveDown/Delete — Admin+Ürün Yöneticisi'ne kapalı, `ProductController`'a action yığılmadı), Product Edit'e "Görseller" bölümü (multipart upload + thumbnail listesi + ana-görsel/sıralama/silme aksiyonları), Product Index'e thumbnail sütunu.
- **RBAC:** Madde 30'un alan-seviyeli kısmi yetki isteği (İçerik Editörü görsel düzenleyebilsin) bu task'ta da **uygulanmadı** — Task 5 kararı sessizce genişletilmedi, aynı gerekçeyle korundu.
- **Migration:** `AddProductImages` — yalnızca `ProductImages` tablosu + filtered unique index + Cascade FK; Identity/Languages/Translations/Categories/Collections/Products'a dokunulmadı.
- **Doğrulama (34/34 iş kuralı/güvenlik senaryosu ✅):** Geçerli jpg/png/webp yükleme, geçersiz uzantı reddi, uzantı/MIME uyuşmazlığı reddi, boş dosya reddi, boyut aşımı reddi, magic-byte imza uyuşmazlığı reddi, var olmayan ürün reddi, ana görsel otomasyonu (ilk yükleme), ana-görsel-tekilliği (SetPrimary eskisini kaldırır), sıralama (MoveUp), çapraz-ürün-izolasyonu (productId manipülasyonu reddedilir), silme (DB+fiziksel dosya), ana-görsel-silme-fallback'i, ürün silme kaskadı (DB+disk) — tamamı geçici doğrulama koduyla (`ProductImageService` doğrudan çağrılarak, HTTP katmanı bypass edilerek) test edildi. sqlcmd ile şema + tam temizlik (Products/ProductImages/Categories/Collections/Translations tümü 0 satır, Identity etkilenmedi) doğrulandı. Uygulama gerçekten başlatılıp 6 endpoint'te (`/Product`, `/Product/Edit/1`, `/ProductImage/Upload|SetPrimary|MoveUp|Delete`) anonim erişimin Login'e yönlendirdiği curl ile doğrulandı; unhandled exception yok. **Rol-bazlı canlı çoklu-kullanıcı testi ve gerçek multipart HTTP upload testi, credential paylaşılmadığı için yapılamadı** — bunun yerine iş mantığı servis katmanında doğrudan (HTTP'yi bypass ederek) test edildi, controller/routing/antiforgery/RBAC doğruluğu kod incelemesi + build + anonim-erişim testleriyle doğrulandı.
- Geçici doğrulama kodu (`ProductImageVerification.cs` + `Program.cs`'teki `--verify-product-images` bloğu) ve geçici `wwwroot/uploads` test klasörleri tamamen kaldırıldı.
- **ADR-013** olarak `ARCHITECTURE_DECISIONS.md`'ye eklendi — ADR-006/ADR-011'i tamamlar, değiştirmez; sonraki dosya/görsel içeren modüller (#9 Katalog/Doküman, Blog, Banner, Referans Proje) bu deseni tekrar kullanabilir.
- Backlog madde #8 ("Ürün Görselleri") **tamamen tamamlandı** işaretlendi.
- **Bilinçli olarak yapılmayanlar:** Merkezi/generic medya kütüphanesi, Blob/S3/CDN, image-processing/otomatik crop, drag-drop (yukarı/aşağı butonları yeterli görüldü), toplu zip yükleme, Excel import, public site API, alan-seviyeli RBAC, video desteği, alt metin/çoklu dil metadata, otomatik test projesi.

---

## Task 6 — Katalog/Doküman Yönetimi (TAMAMLANDI, 19.07.2026)

**Kritik bulgu 1 — Çoklu dil (Translation KULLANILMADI, doküman-içi tutarlılıkla çözüldü):** Madde 24'ün `Document` tablosu hem `DocumentName`'i multi-lang işaretliyor hem ayrı bir `Language` alanı listeliyor. Madde 18.3'ün dosya isimlendirme standardı ("..._dil.pdf") çözümü veriyor: her fiziksel PDF tek bir dile ait — TR/EN sürümleri ayrı `Document` satırlarıdır. `Document.Title` native sütun (Translation'a taşınmadı), `Document.LanguageId` satırın dilini belirtiyor. `EntityType.Document` eklenmedi.

**Kritik bulgu 2 — İlişki modeli (dokümanda açıkça yazılı):** Madde 36.1/36.2, `Document`↔`Product`/`Collection`'ın **many-to-many** olduğunu (`ProductDocuments`/`CollectionDocuments` junction adlarıyla) ve "genel seviye" (ilişkisiz) dokümanın mümkün olduğunu (Madde 24) açıkça belirtiyor.

**Kritik bulgu 3 — Silme davranışı + klasör yapısı sapması (ADR-014):** Product/Collection silme → yalnızca ilişki satırı silinir (junction FK'leri Cascade), Document+fiziksel dosya korunur. ADR-006/Madde 35.4'ün literal `/products/{urunKodu}/...` klasör örneği M2M/opsiyonel cardinality için yapısal olarak uygulanamadığından `/uploads/documents/{tipSegmenti}/{guid}.pdf` kullanıldı — ADR-006'nın ilkesi korunuyor, yalnızca product-code öneki atlandı.

**Yan-bulgu ve düzeltme (bu task sırasında keşfedildi):** `CategoryService.DeleteAsync`/`CollectionService.DeleteAsync`, Task 5'in `Product.CategoryId`/`CollectionId` (Restrict FK) referanslarını kontrol etmiyordu — kullanımdaki kategori/koleksiyon silinmeye çalışılınca **uygulama çöküyordu** (EF Core değişiklik-izleyicisi hatası). `IProductRepository.HasAnyWithCategoryIdAsync`/`HasAnyWithCollectionIdAsync` eklenip her iki serviste kontrol edildi; doğrulama testinde bizzat tetiklenip düzeltmesi de test edildi (54 testin 3'ü bu düzeltmeye ait).

- **Domain:** `Document` (DocumentType enum: Catalog/TechnicalSheet/Certificate/Report; Title native; LanguageId/Language FK Restrict; FilePath/OriginalFileName/FileExtension/ContentType/FileSize; DisplayOrder; IsActive — audit alanı yok), `ProductDocument`/`CollectionDocument` (junction entity'ler, ikisi de Cascade FK + composite unique index).
- **Application:** `Application/Documents/` (`IDocumentRepository`, `DocumentService`, `DocumentDto`, `DocumentRequests`, `DocumentOperationResult`, `DocumentEnumDisplay`). PDF doğrulama: uzantı `.pdf`, MIME `application/pdf`, magic-byte `%PDF-`, boş dosya reddi, ≤20MB (MVP kararı, ProductImage'ın 5MB'ından yüksek — kataloglar doğal olarak büyük). İlişki yönetimi diff-based upsert (`ReplaceProductRelationsAsync`/`ReplaceCollectionRelationsAsync`). Dosya değiştirme + telafi mantığı ProductImage ile birebir desen. **Mevcut `IFileStorageService`/`LocalFileStorageService` (Task 5.1) hiç değiştirilmeden tekrar kullanıldı** — arayüz zaten tamamen generic'ti, ADR-013'ün öngörüsü doğrulandı.
- **Infrastructure:** `DocumentConfiguration`, `ProductDocumentConfiguration`, `CollectionDocumentConfiguration`, `DocumentRepository`.
- **Presentation:** `DocumentController` (Index/Create/Edit/ToggleActive/Delete), Product/Collection çoklu-seçim (`<select multiple>`), Dil/DocumentType dropdown'ları, dosya linkleri doğrudan `/uploads/...` web-relative yoluna (Task 5.1'deki gibi, ayrı Download action'ı yok).
- **RBAC — Madde 30'a literal sadakat (Task 5/5.1'den bilinçli farklı):** Doküman bu modül için açık satır içeriyor: Admin=Tam, İçerik Editörü=yalnızca Create/Yükleme, SEO Editörü=hiç erişim yok, Ürün Yöneticisi=Tam (silme dahil). Task 5/5.1'in "alan-seviyeli RBAC'ı sessizce genişletme" yasağı ihlal edilmedi — burada action-seviyeli bir ayrım (ayrı Create action'ı zaten var) doğrudan uygulanabilir olduğu için hayata geçirildi.
- **Migration:** `AddDocuments` — yalnızca `Documents`/`ProductDocuments`/`CollectionDocuments`; Identity/Languages/Translations/Categories/Collections/Products/ProductImages'a dokunulmadı.
- **Doğrulama (54/54 iş kuralı/güvenlik senaryosu ✅):** Geçerli PDF+2 ürün+1 koleksiyon ilişkisiyle yükleme, geçersiz uzantı/MIME/magic-byte/boş/boyut-aşımı reddi, Title/Language/DisplayOrder/ilişki-var-olma doğrulaması, genel-ilişkisiz doküman (farklı dilde), metadata-only edit, dosya değiştirme+eski-dosya-temizliği, toggle active, **Product silme → Document hayatta kaldı + fiziksel dosya korundu** (kritik davranış testi), **Collection silme → aynı sonuç**, kullanımdaki kategori/koleksiyon silme reddi (yan-bulgu düzeltmesi), çapraz-doküman izolasyonu, silme DB+disk temizliği — tamamı geçici doğrulama koduyla (`DocumentService` doğrudan çağrılarak) test edildi. sqlcmd ile şema + tam temizlik doğrulandı. Uygulama gerçekten başlatılıp 6 endpoint'te anonim erişimin Login'e yönlendirdiği curl ile doğrulandı.
- Geçici doğrulama kodu (`DocumentVerification.cs` + `--verify-documents` bloğu) ve geçici `wwwroot/uploads` test klasörleri tamamen kaldırıldı. **Not:** İlk doğrulama koşusu, script'in kendi (o zaman henüz keşfedilmemiş) collection-delete-guard bug'ına çarpıp beklenmedik şekilde sonlandı — kalıntı test verisi manuel sqlcmd ile temizlendi, düzeltme uygulanıp script güncellendikten sonra ikinci koşu 54/54 temiz geçti.
- **ADR-014** olarak `ARCHITECTURE_DECISIONS.md`'ye eklendi — ADR-006/ADR-013'ü tamamlar, klasör yapısı sapmasını gerekçelendirir.
- Backlog madde #9 ("Katalog/Doküman Yönetimi") **tamamen tamamlandı** işaretlendi.
- **Bilinçli olarak yapılmayanlar:** Otomatik thumbnail/kapak görseli üretimi (PDF-render kütüphanesi gerektirir, Madde 24 "otomatik üretilebilir" diyor — MVP'de atlandı), duplicate-title kontrolü (doküman istemiyor), indirme sayacı/yayın tarihi (Madde 24'te yok), ayrı Download controller action'ı, alan-seviyeli RBAC'ın Task 6'nın uyguladığından daha fazla genişletilmesi, otomatik test projesi.

---

# 3. Şu An Tam Olarak Nerede Kaldık?

- **Task 11 (Sayfa Yönetimi, Backlog #12) başarıyla tamamlandı ve doğrulandı (19.07.2026).** Domain (`Page`, `PageContentBlock`), Application (`PageService`, `PageContentBlockService`), Infrastructure (repository'ler + configuration'lar), Presentation (`PageController` + `PageContentBlockController` + view'lar) uçtan uca kuruldu; `AddPages` migration'ı uygulandı. `IFileStorageService` **yedinci kez** sıfır değişiklikle tekrar kullanıldı; `EntityType.Page` (Task 3.1B'den beri rezerve, hiç kullanılmamıştı) ilk kez tüketildi, yeni `EntityType.PageContentBlock` eklendi.
- **Kritik bulgular:** (1) Page, projede **ilk kez** IsActive/Status/PublishDate/ParentId/DisplayOrder'ın hepsinden yoksun — doküman (Madde 16.2/17.2/30) Page için hiçbirini istemiyor (önceki tüm CMS modüllerinde en az biri vardı); (2) Madde 16.2'nin 5 blok tipi (TextImage/FullWidthImage/VideoEmbed/Accordion/Tab) tek `PageContentBlock` entity'sinde birleşti, Accordion/Tab'ın çoklu panel/sekme grup yapısı dokümanda tanımlanmadığı için ayrı bir grup kimliği/alt tablo icat edilmedi — her blok bağımsız bir içerik birimi (MVP sınırlaması, gelecek faz); (3) blok tipi değiştirildiğinde eski tipe ait kullanılmayan veri (görsel veya video linki) otomatik temizleniyor — projenin ilk "tip değişiminde çapraz-alan temizliği" kuralı; (4) Madde 30'un SEO Editörü'ne verdiği "Meta Alanları" düzenleme yetkisi, Blog/News/Product/Category/Collection/ReferenceProject'in hiçbirinde alan-seviyeli RBAC olmadığı doğrulandıktan sonra, aynı konvansiyonla action-seviyeli salt-görüntüleme'ye indirgendi — **açık teknik borç**.
- **Yan-bulgu yok** — Page↔PageContentBlock **bire-çok (one-to-many)**, tek-sahipli Cascade FK (ProductImage/ReferenceProjectImage deseni — bir Page birden fazla PageContentBlock içerebilir, her blok tek bir Page'e aittir), Restrict FK yok. *(Düzeltme, Task 12: önceki kapanış raporunda "1:1-sahipli" ifadesi yanlış anlaşılmaya açıktı — "tek-sahip" anlamındaydı, gerçek kardinalite her zaman bire-çoktu; kod incelemesiyle doğrulandı, `WithMany()` + non-unique `IX_PageContentBlocks_PageId` index.)*
- 45/45 iş kuralı/güvenlik doğrulama senaryosu geçti (5 blok tipinin her biri, tip-değişimi-temizliği, FullWidthImage/VideoEmbed zorunluluk reddi, sayfa silme kaskadı dahil). Rol-bazlı çoklu-kullanıcı canlı testi bu oturumda da credential eksikliği nedeniyle yapılamadı; anonim erişim + RBAC 6 endpoint'te curl ile doğrulandı.
- Geçici doğrulama kodu ve test klasörleri tamamen temizlendi.
- Yarım kalan hiçbir geliştirme yok; son çalışan/derlenen/test edilen durum Task 11'in tamamlandığı andır (build 0 Warning/0 Error).
- Sıradaki iş: Faz 1 backlog'undaki bir sonraki modül (SEO veri sözleşmesi #4, alan-seviyeli RBAC task'ı #23, veya Bayi/Showroom Yönetimi #11) — henüz başlanmadı, onay bekliyor.

---

## Önceki Durum (Task 10, Banner Yönetimi) — TAMAMLANDI (19.07.2026)

- **Task 10 (Banner Yönetimi, Backlog #13) başarıyla tamamlandı ve doğrulandı (19.07.2026).** Domain (`Banner`), Application (`BannerService`), Infrastructure (`BannerRepository`+configuration), Presentation (`BannerController`+view'lar) uçtan uca kuruldu; `AddBanner` migration'ı uygulandı. `IFileStorageService` **altıncı kez** sıfır değişiklikle tekrar kullanıldı; `EntityType.Banner` (Task 3.1B'den beri rezerve, hiç kullanılmamıştı) ilk kez tüketildi.
- **Kritik bulgular:** (1) Madde 16.1 Banner için Blog/News'ten farklı üç somut gereksinim veriyor — **sıralama** (DisplayOrder eklendi, Blog/News'te yoktu), **2-durumlu aktif/pasif** (bool IsActive+ToggleActive, Blog/News'in 3-durumlu Status enum'ından bilinçli farklı), **yayın tarihi aralığı** (Start+End, Blog/News'in tekil PublishDate'inden farklı); (2) doküman "görsel/video yükleme" diyor ama video desteği mevcut doğrulama altyapısının kapsamı dışında bırakıldı (yalnızca görsel); (3) Madde 17.2/36.1 Banner için SEO hiç anmadığı için SEO alanları eklenmedi (Blog/News'ten farklı); (4) Madde 30 Banner satırı SEO Editörü'ne salt-görüntüleme bile vermiyor (Blog/News'ten farklı, SEO alanı olmadığı için tutarlı).
- **Yan-bulgu yok** — Banner hiçbir ilişkiye sahip değil (Category/Tag/Product yok), en yalın modül.
- 33/33 iş kuralı/güvenlik doğrulama senaryosu geçti (yayın tarihi aralığı geçersizliği reddi dahil). Rol-bazlı çoklu-kullanıcı canlı testi bu oturumda da credential eksikliği nedeniyle yapılamadı; anonim erişim + RBAC 5 endpoint'te curl ile doğrulandı.
- Geçici doğrulama kodu ve test klasörleri tamamen temizlendi.
- Yarım kalan hiçbir geliştirme yok; son çalışan/derlenen/test edilen durum Task 10'un tamamlandığı andır (build 0 Warning/0 Error).
- Sıradaki iş: Faz 1 backlog'undaki bir sonraki modül (SEO veri sözleşmesi #4, Sayfa Yönetimi #12, veya Bayi/Showroom Yönetimi #11) — henüz başlanmadı, onay bekliyor.

---

## Önceki Durum (Task 9, Haber Yönetimi) — TAMAMLANDI (19.07.2026)

- **Task 9 (Haber Yönetimi, Backlog #15) başarıyla tamamlandı ve doğrulandı (19.07.2026).** Domain (`News`, `NewsCategory`), Application (`NewsService`, `NewsCategoryService`), Infrastructure (repository'ler + configuration'lar), Presentation (`NewsController` + `NewsCategoryController` + view'lar) uçtan uca kuruldu; `AddNews` migration'ı uygulandı. `IFileStorageService` **beşinci kez** sıfır değişiklikle tekrar kullanıldı.
- **Kritik bulgular:** (1) Madde 22'nin prose metni ("Haber veri modeli blog modülü ile benzer yapıda olacaktır: başlık, içerik, kapak görseli, kategori, yayın tarihi, durum ve SEO alanları") Blog'un 21.1 tablosundaki Excerpt/Author/Tags'i **içermiyor** — bu üçü bilinçli olarak Haber'e kopyalanmadı; (2) Madde 36.1 "NewsCategories" ayrı tablo dediği için `NewsCategory`, `BlogCategory` deseninin birebir tekrarı olarak eklendi (yeni `EntityType.NewsCategory`); (3) News↔NewsCategory nullable FK+SetNull (Blog ile aynı), **M2M ilişki yok** (doküman Haber için ürün/etiket ilişkisi anmıyor); (4) Status değerleri doküman'da verilmediği için Blog'un somut örneği (Taslak/Yayında/Arşiv) esas alınarak yeni, bağımsız `NewsStatus` enum'u oluşturuldu (enum paylaşımı yapılmadı, proje genelinde her zaman ayrı enum kullanılıyor).
- **Karşılaşılan ve çözülen derleme sorunu:** `namespace Application.News` içinde bare `News` kullanımı `Domain.Entities.News` ile ad çakışması yarattı (CS0118) — üç noktada `Domain.Entities.News` tam nitelikli adıyla çözüldü, mimari etkisi yok.
- **Yan-bulgu yok** — News↔NewsCategory tamamen SetNull, Restrict FK yok.
- 31/31 iş kuralı/güvenlik doğrulama senaryosu geçti. Rol-bazlı çoklu-kullanıcı canlı testi bu oturumda da credential eksikliği nedeniyle yapılamadı; anonim erişim + RBAC 7 endpoint'te curl ile doğrulandı.
- Geçici doğrulama kodu ve test klasörleri tamamen temizlendi (sqlcmd ile News/NewsCategories tablolarının 0 satır olduğu doğrulandı — bu modülde Blog'un Tag havuzu gibi kalıcı paylaşılan bir tablo olmadığı için manuel ek temizlik gerekmedi).
- Yarım kalan hiçbir geliştirme yok; son çalışan/derlenen/test edilen durum Task 9'un tamamlandığı andır (build 0 Warning/0 Error).
- Sıradaki iş: Faz 1 backlog'undaki bir sonraki modül (SEO veri sözleşmesi #4, Banner Yönetimi #13, veya Bayi/Showroom Yönetimi #11) — henüz başlanmadı, onay bekliyor.

---

## Önceki Durum (Task 8, Blog Yönetimi) — TAMAMLANDI (19.07.2026)

- **Task 8 (Blog Yönetimi, Backlog #14 — kullanıcı isteğinde "#11" denilmişti, TASKS.md'nin taslak numaralandırmasında Blog #14/Bayi-Showroom #11'dir, modül adı açık olduğu için durdurma nedeni sayılmadı) başarıyla tamamlandı ve doğrulandı (19.07.2026).** Domain (`Blog`, `BlogCategory`, `Tag`, `BlogTag`), Application (`BlogService`, `BlogCategoryService`, `ITagRepository`), Infrastructure (repository'ler + configuration'lar), Presentation (`BlogController` + `BlogCategoryController` + view'lar) uçtan uca kuruldu; `AddBlog` migration'ı uygulandı. `IFileStorageService` **dördüncü kez** sıfır değişiklikle tekrar kullanıldı.
- **Kritik bulgular:** (1) Madde 28.2 "kategori adı"nı genel olarak çoklu-dil-gerektiren alanlar arasında saydığı için `BlogCategory.Name` Translation'a taşındı, yeni `EntityType.BlogCategory` eklendi (migration gerektirmedi); (2) Madde 21.1'in "FeaturedImage: image" (tekil, galeri değil) ifadesi nedeniyle ayrı bir `BlogImage` tablosu **oluşturulmadı** — `Blog.FeaturedImagePath` doğrudan entity'de, ama yükleme/doğrulama ProductImage ile birebir aynı güvenlik desenini kullanıyor; (3) Tags alanı doküman tarafından "(multi-lang)" işaretlenmediği için native tutuldu (Title/Excerpt/Content'ten bilinçli farklı); (4) Blog↔BlogCategory nullable FK + SetNull (Category/Collection'ın Restrict deseninden bilinçli farklı — Blog.BlogCategoryId doğası gereği opsiyonel).
- **Yan-bulgu yok** — BlogCategory silme davranışı SetNull ile otomatik yönetiliyor, ekstra guard kodu gerekmedi.
- 35/35 iş kuralı/güvenlik doğrulama senaryosu geçti (case-insensitive etiket tekilleştirme dahil). Rol-bazlı çoklu-kullanıcı canlı testi bu oturumda da credential eksikliği nedeniyle yapılamadı; anonim erişim + RBAC challenge 7 endpoint'te curl ile doğrulandı.
- Geçici doğrulama kodu ve test klasörleri tamamen temizlendi; paylaşılan Tag havuzunda kalan 3 test etiketi (tasarım gereği otomatik silinmiyor) manuel sqlcmd ile temizlendi.
- Yarım kalan hiçbir geliştirme yok; son çalışan/derlenen/test edilen durum Task 8'in tamamlandığı andır (build 0 Warning/0 Error).
- Sıradaki iş: Faz 1 backlog'undaki bir sonraki modül (SEO veri sözleşmesi #4, Haber Yönetimi #15, veya Bayi/Showroom Yönetimi #11) — henüz başlanmadı, onay bekliyor.

---

## Önceki Durum (Task 7, Referans Proje Yönetimi) — TAMAMLANDI (19.07.2026)

- **Task 7 (Referans Proje Yönetimi, Backlog #10) başarıyla tamamlandı ve doğrulandı (19.07.2026).** Domain (`ReferenceProject`, `ReferenceProjectImage`, `ProductReferenceProject`), Application (`ReferenceProjectService`, `ReferenceProjectImageService`, `IReferenceProjectRepository`, `IReferenceProjectImageRepository`), Infrastructure (repository'ler + configuration'lar), Presentation (`ReferenceProjectController` + `ReferenceProjectImageController` + view'lar) uçtan uca kuruldu; `AddReferenceProjects` migration'ı uygulandı. `IFileStorageService` **üçüncü kez** sıfır değişiklikle tekrar kullanıldı (ADR-013/ADR-014'ün öngörüsü bir kez daha doğrulandı).
- **Kritik bulgular:** (1) Madde 30 (RBAC) tablosu Referans Proje'yi hiç listelemiyor — Madde 7.2'nin İçerik Editörü tanımı ("...referans proje...") esas alınarak Blog/Haber/Banner ile aynı satır (İçerik Editörü=CRUD dahil silme, SEO Editörü=salt-görüntüleme, Ürün Yöneticisi=erişim yok) uygulandı; (2) Madde 23.1'in "Images: gallery" + "FeaturedImage: image" ayrımı, ProductImage'ın `IsPrimary` desenindeki gibi tek `IsFeatured` bayrağıyla tek tabloda (`ReferenceProjectImage`) birleştirildi — ProductImage'ın 5 görsel-tipi modeli buraya taşınmadı, doküman tip ayrımı istemiyor; (3) Madde 23.1 tablosunda Product/Document'ın aksine Zorunluluk sütunu yok — Müşteri Notu ("arşiv yok, altyapı hazır olmalı") esas alınarak yalnızca TR ad+tip zorunlu tutuldu, geri kalan her şey opsiyonel. Ayrıntı TASKS.md "Task 7" bölümünde.
- **Yan-bulgu yok** — ReferenceProject↔Product ilişkisi tamamen M2M/Cascade (Restrict FK hiçbir yönde yok), bu yüzden Task 6'daki gibi bir silme-guard düzeltmesi gerekmedi.
- 38/38 iş kuralı/güvenlik doğrulama senaryosu geçti (minimal "boş arşiv" oluşturma senaryosu dahil). Rol-bazlı çoklu-kullanıcı canlı testi bu oturumda da credential eksikliği nedeniyle yapılamadı; anonim erişim + RBAC challenge (302) 5 endpoint'te curl ile doğrulandı.
- Geçici doğrulama kodu, test verisi ve geçici upload klasörleri tamamen temizlendi.
- Yarım kalan hiçbir geliştirme yok; son çalışan/derlenen/test edilen durum Task 7'nin tamamlandığı andır (build 0 Warning/0 Error).
- Sıradaki iş: Faz 1 backlog'undaki bir sonraki modül (SEO veri sözleşmesi #4 veya Bayi/Showroom Yönetimi #11) — henüz başlanmadı, onay bekliyor.

---

## Önceki Durum (Task 6, Katalog/Doküman Yönetimi) — TAMAMLANDI (19.07.2026)

- **Task 6 (Katalog/Doküman Yönetimi) başarıyla tamamlandı ve doğrulandı (19.07.2026).** Domain (`Document`, `ProductDocument`, `CollectionDocument`), Application (`DocumentService`, `IDocumentRepository`), Infrastructure (`DocumentRepository` + configuration'lar), Presentation (`DocumentController` + view'lar) uçtan uca kuruldu; `AddDocuments` migration'ı uygulandı. Task 5.1'in `IFileStorageService`'i hiç değiştirilmeden tekrar kullanıldı — ADR-013'ün "sonraki modüller bu deseni tekrar kullanabilir" öngörüsü doğrulandı.
- **Kritik bulgular:** (1) Document.Title Translation'a değil native sütuna taşındı — her PDF tek bir dile ait; (2) Document↔Product/Collection ilişkisi M2M ve opsiyonel; (3) Product/Collection silme Document'ı etkilemiyor, klasör yapısı ADR-006'nın literal örneğinden gerekçeli şekilde sapıyor (ADR-014). Ayrıntı madde 2'deki "Task 6" bölümünde.
- **Yan-bulgu:** Bu task sırasında Category/Collection silme akışında gerçek bir çökme hatası bulunup düzeltildi (kullanımdaki kategori/koleksiyon artık anlaşılır hata mesajıyla reddediliyor, çökmüyor).
- 54/54 iş kuralı/güvenlik doğrulama senaryosu geçti. Rol-bazlı çoklu-kullanıcı canlı testi ve gerçek HTTP multipart upload testi bu oturumda da credential eksikliği nedeniyle yapılamadı (iş mantığı servis katmanında doğrudan test edildi).
- Geçici doğrulama kodu, test verisi ve geçici upload dosyaları tamamen temizlendi.
- Yarım kalan hiçbir geliştirme yok; son çalışan/derlenen/test edilen durum Task 6'nın tamamlandığı andır (build 0 Warning/0 Error).

---

## Önceki Durum (Task 5, Ürün Yönetimi çekirdek CRUD) — TAMAMLANDI (19.07.2026)

- **Task 5 (Ürün Yönetimi, çekirdek CRUD) başarıyla tamamlandı ve doğrulandı (19.07.2026).** Domain (`Product`), Application (`ProductService`, `IProductRepository` — mevcut `ICategoryRepository`/`ICollectionRepository`/`ITranslationService`/`IUnitOfWork` değiştirilmeden tekrar kullanıldı), Infrastructure, Presentation (`ProductController` + view'lar) uçtan uca kuruldu; `AddProducts` migration'ı uygulandı.
- **Kritik bulgu:** Madde 18.1'in `SeriesName` alanı `Product.CollectionId` FK'sine bağlandı, ayrı `CollectionName` metin alanı eklenmedi — ayrıntı yukarıdaki "Task 5" bölümünde. Product hem `CategoryId` hem `CollectionId`'yi bağımsız kardeş FK olarak taşıyor.
- **Bilinçli RBAC kapsam sınırlaması:** Madde 30'un alan-seviyeli kısmi yetkileri (İçerik Editörü: açıklama/görsel; SEO Editörü: meta alanları) uygulanmadı, mevcut action-seviyeli desen kullanıldı — ayrıntı yukarıda.
- 20/20 iş kuralı doğrulama senaryosu geçti. Rol-bazlı çoklu-kullanıcı canlı testi bu oturumda da credential eksikliği nedeniyle yapılamadı.
- Geçici doğrulama kodu ve test verisi tamamen temizlendi, kalıcı kodda hiçbir iz yok.
- Yarım kalan hiçbir geliştirme yok; son çalışan/derlenen/test edilen durum Task 5'in tamamlandığı andır (build 0 Warning/0 Error).
- Sıradaki iş: Faz 1 backlog'undaki bir sonraki modül (Referans Proje Yönetimi #10 veya SEO veri sözleşmesi #4) — henüz başlanmadı, onay bekliyor.

---

# 4. Bekleyen İşler

| Modül/İş | Durum |
|---|---|
| Translation veri modeli | **TAMAMLANDI (ADR-012 + Task 3.1B, 19.07.2026)** — şema, entity, migration, doğrulama hepsi bitti |
| Language yönetimi (altyapı) | **TAMAMLANDI (Task 3.1B)** — `Language` entity + seed edildi (7 dil). "Dil Yönetimi" panel modülünün kendi CRUD ekranı (Madde 17.2 #12) ayrı, henüz başlanmamış bir iş |
| Translation mimarisi (nihai şema — FieldName/Value/EntityType temsili/unique index) | **TAMAMLANDI** — hem karar (ADR-012) hem implementasyon (Task 3.1B) bitti |
| İlk domain entity | **TAMAMLANDI** — `Language`/`Translation` (Task 3.1B) |
| İlk domain migration | **TAMAMLANDI** — `AddTranslationInfrastructure`, uygulandı ve doğrulandı |
| Kategori Yönetimi | **TAMAMLANDI (Task 4.1, 18-19.07.2026)** — uçtan uca (entity/Application/Infrastructure/Presentation/migration), doğrulandı |
| Koleksiyon Yönetimi | **TAMAMLANDI (Task 4.2, 19.07.2026)** — uçtan uca, Category ile FK ilişkisi yok, doğrulandı |
| İlk Application katmanı kullanımı / ilk admin CRUD ekranı | **TAMAMLANDI (Task 4.1)** — `CategoryService` + `CategoryController`, Task 4.2/5'te aynı desen (`CollectionService`/`ProductService`) sıfır yeni abstraction ile tekrar kullanıldı |
| Ürün Yönetimi (çekirdek CRUD) | **TAMAMLANDI (Task 5, 19.07.2026)** — `Product` hem `CategoryId` hem `CollectionId` FK taşıyor, doğrulandı. Doküman/proje ilişkileri, Excel import, arama/filtreleme **Başlanmadı** (ayrı backlog maddeleri #9/#10/#17). Görsel ilişkisi Task 5.1 ile tamamlandı |
| Ürün Görselleri Yönetimi | **TAMAMLANDI (Task 5.1, 19.07.2026)** — `ProductImage` + `IFileStorageService`/ADR-013, ilk gerçek dosya yükleme özelliği, doğrulandı |
| Dosya Yönetimi (storage abstraction implementasyonu) | **TAMAMLANDI (Task 5.1)** — `IFileStorageService`/`LocalFileStorageService`, ADR-013 olarak kaydedildi. Task 6'da (Katalog/Doküman) sıfır değişiklikle tekrar kullanıldı, ADR-014 ile doğrulandı |
| Katalog/Doküman Yönetimi | **TAMAMLANDI (Task 6, 19.07.2026)** — `Document`+`ProductDocument`/`CollectionDocument` (M2M), ADR-014, doğrulandı |
| Referans Proje Yönetimi | **TAMAMLANDI (Task 7, 19.07.2026)** — `ReferenceProject`+`ReferenceProjectImage`+`ProductReferenceProject` (M2M), `IFileStorageService` üçüncü kez tekrar kullanıldı, doğrulandı |
| Blog Yönetimi | **TAMAMLANDI (Task 8, 19.07.2026)** — `Blog`+`BlogCategory`+`Tag`+`BlogTag` (M2M), `IFileStorageService` dördüncü kez tekrar kullanıldı, `EntityType.BlogCategory` eklendi, doğrulandı |
| Haber Yönetimi | **TAMAMLANDI (Task 9, 19.07.2026)** — `News`+`NewsCategory`, `IFileStorageService` beşinci kez tekrar kullanıldı, `EntityType.NewsCategory` eklendi, doğrulandı |
| Banner Yönetimi | **TAMAMLANDI (Task 10, 19.07.2026)** — tek ilişkisiz `Banner` entity'si, `IFileStorageService` altıncı kez tekrar kullanıldı, `EntityType.Banner` ilk kez tüketildi, doğrulandı |
| Sayfa Yönetimi | **TAMAMLANDI (Task 11, 19.07.2026)** — `Page`+`PageContentBlock` (5 blok tipi), `IFileStorageService` yedinci kez tekrar kullanıldı, `EntityType.Page` ilk kez tüketildi + `EntityType.PageContentBlock` eklendi, doğrulandı. SEO Editörü'nün alan-seviyeli meta-alan yetkisi teknik borç (bkz. Bilinen Riskler) |
| Alan-seviyeli RBAC altyapısı | Başlanmadı (yeni backlog maddesi #23 — Task 5/11'in bıraktığı teknik borç) |
| Bayi / Showroom | **TAMAMLANDI (Task 14, 19.07.2026)** — `Dealer` entity + nullable `Category` enum, Translation/görsel kullanılmıyor, RBAC Admin-only, doğrulandı |
| SEO | Başlanmadı |
| Form Yönetimi | **TAMAMLANDI (Task 15, 20.07.2026)** — `FormSubmission` (tek entity, FormType discriminator), gerçek SQL pagination/filtreleme (ADR-015), RBAC Görüntüleme/Tam ayrımı, doğrulandı. Public gönderim endpoint'i + e-posta bildirimi public site fazına bırakıldı |
| Dashboard (asıl modül — istatistik/özet ekranı) | **TAMAMLANDI (Task 18, 20.07.2026)** — `IDashboardService`+`HomeController.Index`, 8 kart + Son 5 Ürün/Form, doğrulandı |
| Excel Import | Başlanmadı |
| Çoklu Dil (public site fallback davranışı) | Başlanmadı |
| SAP/CRM API Fazı | Başlanmadı (Faz 2 kapsamında, ADR-010 ile bu fazın tamamen dışında) |
| Kullanıcı Yönetimi (liste/oluştur/düzenle/rol ata/aktif-pasif/parola sıfırla/sil) | **TAMAMLANDI (Task 16/16B, 20.07.2026)** — `IUserManagementService`+`UserController`, guardrail'ler, doğrulandı |
| Role Management (sabit 4 rol için salt-okunur denetim ekranı) | **TAMAMLANDI (Task 17, 20.07.2026)** — `IRoleManagementService`+`RoleController` (Index/Details GET-only), statik yetki matrisi, doğrulandı |
| Dinamik rol CRUD / claim / permission altyapısı | **Kapsam dışı (kalıcı) — Task 17 analiziyle netleşti.** Dokümanda hiçbir dayanağı yok (Madde 7.2 dört rolü kapalı liste olarak tanımlıyor) ve mevcut RBAC tamamen derleme-zamanı `[Authorize(Roles=...)]` sabitlerine dayandığından yeni bir rol otomatik erişim kazanamaz — "Başlanmadı" değil, açılmayacak bir kapsam |
| Testing Foundation (kalıcı otomatik test altyapısı) | **TAMAMLANDI (Task 12, 19.07.2026)** — 88/88 test (51 unit + 37 integration), `TESTING.md`, seeder testleri, config-driven migration/seed politikası, migration/index audit + 1 corrective migration. Docker dosyaları oluşturuldu ama **canlı `docker compose up` doğrulaması bu makinede yapılamadı** (Docker Desktop çalışır hale getirilemedi — açık ortam kısıtı) |

---

# 5. Mimari Kararlar (Kesinleşenler Özeti)

- **MVC:** Server-rendered ASP.NET Core MVC (Views/Controllers). SPA/React/Vue/Angular/Razor Pages **yok**.
- **Layered Architecture:** Presentation / Application / Domain / Infrastructure — Domain framework-bağımsız, sıfır paket.
- **Identity:** ASP.NET Core Identity (`AddIdentity<ApplicationUser, IdentityRole>`), Identity.UI/`AddDefaultUI` **kullanılmadı**.
- **Cookie Authentication:** Tek authentication yöntemi; JWT **yalnızca ileride SAP/CRM API fazında** değerlendirilecek.
- **EF Core Code First:** Migration'lar CLI ile (`dotnet ef`), `Database.MigrateAsync()` kodda **kullanılmıyor**.
- **SQL Server:** `.\SQLEXPRESS` (development), veritabanı adı `NGKutahyaSeramikAdminPanel`.
- **Translation yaklaşımı (TAMAMLANDI — hem karar hem implementasyon):** ADR-007 ile merkezi Translations (EntityType+EntityId polimorfik ilişki) ilke, **ADR-012 ile nihai şema** (FieldName düz nvarchar/const string; Value tek nvarchar(max); alan-bazlı ayrı satır; EntityType enum+açık ValueConverter; unique index (EntityType,EntityId,LanguageId,FieldName); yetim kayıt temizliği Application katmanında) onaylı, **Task 3.1B (19.07.2026) ile koda geçirildi ve doğrulandı** (`Domain/Entities/Translation.cs`, `AddTranslationInfrastructure` migration'ı uygulandı). Entity-bazlı ayrı çeviri tabloları reddedildi.
- **Language yaklaşımı (TAMAMLANDI):** Ayrı bir `Language` entity'si (`Id`,`Code`,`Name`,`IsActive`,`DisplayOrder`,`Translations` navigation collection); `Translation.LanguageId`→`Language.Id` **surrogate FK** (doğal anahtar/Code FK değil); `Language.Code` ayrı unique index; `ON DELETE RESTRICT` (dil silinmez, `IsActive=false` ile pasif yapılır). `Code` normalizasyonu Domain'de değil, Application katmanında yapılacak (Identity'nin `ILookupNormalizer` deseniyle tutarlı). **Task 3.1B ile koda geçirildi**, 7 dil (TR/EN/DE/FR/ES/AR/RU) seed edildi.
- **Generic Repository kullanılmıyor.**
- **BaseEntity kullanılmıyor.**
- **Soft Delete kullanılmıyor.**
- **Named Policy kullanılmıyor** — roller doğrudan `[Authorize(Roles = ApplicationRoles.X)]` ile kontrol ediliyor.
- **JWT ertelendi** — yalnızca gelecekteki SAP/CRM entegrasyon fazında değerlendirilecek.
- **Dosya sistemi yaklaşımı:** Yerel Dosya Sistemi + storage abstraction (ADR-006) — abstraction interface'i **henüz yazılmadı**, bu bir sonraki dosya-yönetimi task'ında ele alınacak.
- **Bayi/Showroom:** Tek `Dealer` entity + `Category` alanı (ADR-008) — **TAMAMLANDI (Task 14, 19.07.2026).**
- **Public site / SAP-CRM:** Bu fazın tamamen dışında (ADR-001, ADR-002, ADR-009, ADR-010); mimari bunlara "genişletilebilir" bırakılıyor ama şimdiden hiçbir iskelet/kod üretilmiyor.

---

# 6. Test Durumu

**Task 12 (19.07.2026) ile proje kalıcı otomatik test altyapısına kavuştu; Task 13 (19.07.2026) News/NewsCategory'yi ekleyerek genişletti — bu bölüm buna göre güncellendi.**

**Kalıcı otomatik testler (`tests/NGKutahyaSeramik.UnitTests` + `tests/NGKutahyaSeramik.IntegrationTests`, toplam 131/131 ✅):**
- **Unit (75):** `BlogServiceTests`, `PageServiceTests`, `PageContentBlockServiceTests`, `ProductServiceTests`, `NewsServiceTests`, `NewsCategoryServiceTests` (Task 13 — 16+8 test: TR başlık/ad zorunluluğu, translation upsert/temizlik, kategori doğrulama, kategori-silme→SetNull, Status/PublishDate, kapak görseli yükleme/değiştirme/kaldırma, duplicate-ad reddi, toggle), `IdentitySeederTests`, `LanguageSeederTests`. Gerçek SQLite in-memory + gerçek repository/UnitOfWork ("sociable unit test"), yalnızca dosya I/O ve Translation persistence sahte (`FakeFileStorageService`/`FakeTranslationService`). `UseInMemoryDatabase` **hiçbir testte kullanılmadı**.
- **Integration (56):** `AnonymousAccessTests` (anonim erişim → Login'e yönlendirme, News/NewsCategory dahil), `RbacTests` (5 istemci tipi: Admin/İçerik Editörü/SEO Editörü/Ürün Yöneticisi/yetkisiz-authenticated, News/NewsCategory dahil), `AntiForgeryAndPrgTests` (token'sız POST → 400, başarılı POST → 302 PRG, News dahil — AntiForgery **hiçbir testte devre dışı bırakılmadı**), `RelationalConstraintTests` (gerçek FK/SetNull/Cascade/unique constraint davranışı, News→NewsCategory SetNull dahil, `WebApplicationFactory<Program>` ile).
- Ayrıntılı altyapı/desen açıklaması: `TESTING.md`.

**Diğer, testten bağımsız gerçek çalıştırma doğrulamaları (bu oturumda tekrarlandı):**
- `dotnet clean` → `dotnet restore` → `dotnet build` (temiz build) → 0 Warning/0 Error.
- Boş/sıfırdan bir SQL Server veritabanına (`NGKutahyaSeramikAdminPanel_FinalCheck`, geçici) karşı `DatabaseInitialization:ApplyMigrationsOnStartup=true`/`SeedOnStartup=true` ile uygulama iki kez çalıştırıldı — 1. çalıştırma: 29 tablo doğru oluştu, admin+dev test kullanıcısı+4 rol+7 dil seed edildi. 2. çalıştırma: satır sayıları (2 kullanıcı/4 rol/7 dil) ve kullanıcı-checksum'ı (`CHECKSUM_AGG`) birebir aynı kaldı → **duplicate yok, parola/credential overwrite yok**. Geçici veritabanı test sonrası drop edildi.
- Identity seed idempotency (rol + admin + development test kullanıcı) — ikinci çalıştırmalarda duplicate yok, parola/checksum korunuyor (artık ayrıca `IdentitySeederTests` ile de otomatik test ediliyor).
- Authentication akışı uçtan uca (Login/Logout/AccessDenied/ReturnUrl/Open Redirect) — curl ile gerçek HTTP istekleriyle test edildi.
- Role-Based Authorization (Admin/İçerik Editörü ayrımı, organik AccessDenied) — gerçek kullanıcılarla test edildi.
- Production ortamı simülasyonu (development-only seed'in çağrılmadığı) — ampirik olarak kanıtlandı.

**Coverage (Task 13, 19.07.2026 — güncel):** Global 14.1% line / 24.2% branch / 40.7% method (Task 12'nin %11.5'inden yükseldi — News 5. hedeflenen modül oldu). Hedeflenen sınıflarda: `BlogService` 76.3%, `PageService` 93.3%, `PageContentBlockService` 75.5%, `ProductService` 74.9%, `NewsService` 80.5%, `NewsCategoryService` 88.6%, `IdentitySeeder` 80.8%, `LanguageSeeder` 100%. Komut: `dotnet test --collect:"XPlat Code Coverage"` (bkz. `TESTING.md`).

**Henüz yapılmayan testler:**
- Category/Collection/Document/ReferenceProject/Banner servislerinin unit testleri — Task 12 kapsamı Blog/Page/PageContentBlock/Product'ı, Task 13 News/NewsCategory'yi hedefledi (bilinçli "foundation, exhaustive değil" kararı — kademeli genişleme), gelecek task'larda genişletilebilir.
- Gerçek Docker `docker compose up` canlı doğrulaması — bu makinede Docker Desktop çalışır hale getirilemedi (açık ortam kısıtı, bkz. §9).
- Ürün Yönetimi'nin doküman/referans proje ilişkileri, Excel import, arama/filtreleme henüz yok — test edilmedi.
- Gerçek tarayıcı üzerinden multipart dosya yükleme (Integration testlerde `MultipartFormDataContent` ile simüle edildi, gerçek tarayıcı testi değil).
- Rol değişikliğinin cookie'ye yansıması **bilinçli olarak test edilmedi** (kullanıcı kararıyla — DB'de manuel rol değiştirme riskli/gereksiz bulundu).
- Şifre sıfırlama, 2FA, kullanıcı kayıt akışları — hiç implement edilmedi, dolayısıyla test edilmedi (kapsam dışı).
- Rate limiting gerçek değerleri, log hedefleri (dosya/SQL/Seq) — ADR-011'de teknik yaklaşım seçildi ama somut konfigürasyon/test henüz yapılmadı.

**Riskli kalan noktalar:**
- `dotnet-ef` global aracı **10.0.9**, proje paketleri **9.0.18** — şimdiye kadar sorun çıkarmadı ama versiyon farkı izlenmeli.
- Bazı 10.x paketler (`Serilog.AspNetCore`, `Microsoft.Extensions.*.Abstractions`) hâlâ projede duruyor — derlemeyi engellemiyor ama sürüm hijyeni açısından ayrı bir küçük task olarak not edildi, henüz ele alınmadı.
- Translation şemasının nihai tasarımı netleşmeden içerik modüllerine (Kategori/Ürün vb.) başlanırsa yeniden yazım riski var — bu yüzden Dil Yönetimi'nin öncelenmesi öneriliyor.

---

# 7. Bir Sonraki Oturum Nereden Devam Etmeli?

**Translation/Language altyapısı, Kategori (4.1), Koleksiyon (4.2), Ürün çekirdek CRUD (5), Ürün Görselleri (5.1), Katalog/Doküman Yönetimi (6) ve Referans Proje Yönetimi (7) tamamen bitti.** Bir sonraki oturum, Faz 1 backlog'undaki bir sonraki modülle devam edecek: SEO veri sözleşmesi (#4) veya Bayi/Showroom Yönetimi (#11) — bkz. TASKS.md backlog sıralaması, henüz hiçbiri onaylanmadı.

**Kategori/Koleksiyon/Ürün ile kurulan desenler, yeni bir içerik modülü eklenirken doğrudan tekrar kullanılabilir/örnek alınabilir — Task 5, FK-ilişkili bir entity'de; Task 5.1, dosya yükleme gerektiren bir alt-entity'de; Task 6, M2M ilişkili + dosya yükleme gerektiren bağımsız bir entity'de bu desenlerin hiçbir yeni genel-amaçlı abstraction eklemeden çalıştığını kanıtladı:**
1. `Domain/Enums/EntityType.cs`'e ilgili üye eklenir (migration gerekmez, yalnızca kod + `EntityTypeMapping`'e karşılık gelen sabit string) — **yalnızca gerçekten Translation-tabanlı çoklu dil gerektiren entity'ler için**; Task 6, dosya-başına-tek-dil modeli olan entity'lerin (`Document` gibi) Translation'a hiç ihtiyaç duymadığını gösterdi (bkz. madde 8).
2. Modülün kendi `{Modul}Fields.cs` sabitleri tanımlanır (Translation kullanılıyorsa — Karar A deseni).
3. `Domain/Entities/{Modul}.cs` (sade POCO) + gerekirse `Domain/Entities/{Modul}{Iliski}.cs` junction entity'ler (M2M ilişki varsa — Task 6'daki `ProductDocument`/`CollectionDocument` deseni) + `Infrastructure/Persistence/Configurations/{Modul}Configuration.cs` + `I{Modul}Repository`/`{Modul}Repository`.
4. `{Modul}Service` (Application'da; FK/M2M hedefi başka bir modülse o modülün repository'si enjekte edilip var olma kontrolü yapılır — Task 5'te `ICategoryRepository`/`ICollectionRepository`, Task 6'da aynı + M2M diff-upsert ile örneklendi).
5. `{Modul}Controller` + view'lar, `[Authorize(Roles=...)]` RBAC deseni. **Önce Madde 30'un kendi tablosunu kontrol et:** modül için açık bir satır varsa (Task 6'daki Katalog/Doküman gibi) o satıra **literal sadakatle** uygulanır (action-seviyeli ayrım mümkünse, örn. "yalnızca Yükleme" → ayrı Create action + kısıtlı rol); satır yoksa (Task 5'teki Ürün Yönetimi gibi, RBAC tablosunda kendi satırı olsa da alan-seviyeli granülerlik gerektiriyorsa) mevcut action-seviyeli desen kullanılıp gerekçeyle kayıt altına alınır.
6. **Dosya/görsel/PDF içeriyorsa (Task 5.1 ile kurulan, Task 6 ile ikinci kez doğrulanan desen):** Mevcut `Application/Storage/IFileStorageService` + `Infrastructure/Storage/LocalFileStorageService` **doğrudan tekrar kullanılır** — yeni bir storage implementasyonu gerekmez. Entity Product'a/Collection'a **1:1 sahipse** (ProductImage gibi) klasör `{modul}/{sahipKodu}/{tip}/...`; entity **M2M/opsiyonel ilişkiliyse** (Document gibi) klasör `{modul}/{tip}/...` (sahiplik-bağımsız, ADR-014). Hangisi olduğu doküman analiziyle netleştirilmeli, tahmin edilmemeli.
7. Migration + doğrulama + dokümantasyon — tek task içinde birlikte.

**Ürün Yönetimi'nin geri kalanı için özel not:** Task 5 çekirdek `Product` entity'sini, Task 5.1 görsel ilişkisini (#8), Task 6 doküman ilişkisini (#9), Task 7 referans proje ilişkisini (#10) kapsadı. Yalnızca Excel Import (#17) hâlâ ayrı, başlanmamış bir backlog maddesidir.

**Önemli:** Yeni sohbette önce bu `PROGRESS.md` dosyası, ardından gerekirse `PROJECT_MEMORY.md`/`ARCHITECTURE_DECISIONS.md`/`TASKS.md` okunmalı. Hangi içerik modülüyle devam edileceği kullanıcıya sorulmalı — TASKS.md'deki backlog sıralaması bir öneri, henüz hiçbiri onaylanmadı. Referans dokümanı (`NG_Kutahya_Seramik_Kavramsal_Analiz_v2.pdf`) proje klasöründe değil, `Downloads` klasöründe — yeni modülün gereksinimleri için ilgili maddesi doğrudan okunmalı, tahmin edilmemeli.

---

# 8. Çalıştırma Notları

**Ön koşullar:** .NET 9 SDK, SQL Server Express (`.\SQLEXPRESS` instance'ı çalışır durumda), `dotnet-ef` global aracı kurulu.

**Build:**
```
cd NGKutahyaSeramik_AdminPanel
dotnet build
```

**Migration (CLI, proje kökünden):**
```
dotnet ef migrations add <MigrationAdi> --project src/Infrastructure/Infrastructure.csproj --startup-project src/Presentation/Presentation.csproj --output-dir Persistence/Migrations
```

**Database update:**
```
dotnet ef database update --project src/Infrastructure/Infrastructure.csproj --startup-project src/Presentation/Presentation.csproj
```

> Not: Migration/database update komutları **`ASPNETCORE_ENVIRONMENT=Development`** ortam değişkeniyle çalıştırılmalı (aksi halde `appsettings.json`'daki boş connection string nedeniyle design-time host inşası başarısız olur).

**Run (Development):**
```
cd src/Presentation
dotnet run
```
veya belirli bir port için: `dotnet run --urls "http://localhost:5167"`.

**Gerekli User Secrets anahtarları (Presentation projesi, gerçek değerler ASLA buraya yazılmaz):**
- `ConnectionStrings:DefaultConnection` (appsettings.Development.json'da zaten tanımlı, User Secrets'a gerek yok — bu satır sadece referans)
- `SeedAdmin:Email`
- `SeedAdmin:Password`
- `SeedTestUser:Email`
- `SeedTestUser:Password`

Değerleri kontrol etmek için (değerleri EKRANA BASMADAN, sadece anahtar var mı diye): `dotnet user-secrets list` (Presentation klasöründe) — **çıktısı gizli bilgi içerir, dışarı paylaşılmamalı.**

---

# 9. Bilinen Riskler

**Henüz doğrulanmamış noktalar:**
- Rol değiştirildikten sonra mevcut cookie'nin davranışı (belgelendi ama gerçek DB üzerinde canlı test edilmedi).
- `dotnet-ef 10.0.9` (global) ile proje paketlerinin (9.0.18) uzun vadeli tam uyumluluğu.
- Rate limiting/loglama'nın somut konfigürasyonu (ADR-011'de yaklaşım seçildi, gerçek değerler/hedefler henüz uygulanmadı).
- **Kategori, Koleksiyon, Ürün, Ürün Görselleri, Katalog/Doküman ve Referans Proje'de rol-bazlı çoklu-kullanıcı canlı erişim testi (Task 4.1 + 4.2 + 5 + 5.1 + 6 + 7)** — anonim erişim engeli curl ile altı modülde de doğrulandı, ama İçerik Editörü/SEO Editörü/Ürün Yöneticisi rolleriyle gerçek oturum açıp Create/Edit/Delete/Upload butonlarının görünürlüğü ve action-seviyesi erişim reddi canlı test edilmedi (bu oturumlarda gerçek kullanıcı parolaları paylaşılmadı). Mekanizmanın kendisi (`[Authorize(Roles=...)]`) Task 2.1/2.2'de kanıtlanmış, ama her modüle özgü rol matrisi ayrıca canlı doğrulanmalı.
- **Ürün Yönetimi ve Ürün Görselleri'nde Madde 30'un istediği alan-seviyeli kısmi RBAC uygulanmadı (Task 5 + 5.1, bilinçli kapsam sınırlaması)** — İçerik Editörü'nün yalnızca "Açıklama/Görsel", SEO Editörü'nün yalnızca "Meta Alanları" düzenleyebilmesi gerekiyordu; bunun yerine mevcut action-seviyeli desen (Admin+Ürün Yöneticisi tam, diğerleri salt görüntüleme) kullanıldı. Gerekçe ve kapsam PROJECT_MEMORY.md "Task 5"/"Task 5.1" bölümlerinde kayıtlı; ileride ayrı bir task olarak ele alınmalı. **Katalog/Doküman (Task 6) istisna** — Madde 30'un o modül için verdiği açık RBAC satırına literal sadakatle uyuldu (İçerik Editörü: yalnızca Yükleme), çünkü action-seviyeli ayrım (ayrı Create action) doğrudan uygulanabilirdi.
- **Ürün Görselleri ve Katalog/Doküman'da gerçek HTTP multipart upload'ı test edilmedi (Task 5.1 + 6)** — `ProductImageService`/`DocumentService`'in iş mantığı doğrudan (HTTP'yi bypass ederek) test edildi (34/34, 54/54) ama tarayıcı/curl üzerinden authenticated gerçek dosya yükleme akışı credential kısıtı nedeniyle canlı doğrulanamadı. Controller/routing/antiforgery/RBAC doğruluğu kod incelemesi + build + anonim-erişim testleriyle doğrulandı.
- **Sayfa Yönetimi'nde Madde 30'un istediği alan-seviyeli "Meta Alanları" RBAC uygulanmadı (Task 11, açık teknik borç — Task 5'teki aynı sınırlamanın devamı).** SEO Editörü'nün yalnızca SeoUrl/MetaTitle/MetaDescription'ı düzenleyebilmesi gerekiyordu; Blog/News/Product/Category/Collection/ReferenceProject controller'ları incelendi, hiçbirinde alan-seviyeli RBAC altyapısı olmadığı doğrulandı — bu yüzden Page'de de mevcut action-seviyeli desen (SEO Editörü yalnızca görüntüler, hiçbir alanı düzenleyemez) korundu. **Yeni backlog maddesi #23 ("Alan-seviyeli RBAC altyapısı") olarak TASKS.md'ye eklendi** — bu task tamamlanmadan SEO Editörü ne Page'de ne Product'ta meta alanlarını düzenleyebilecek.
- **PageContentBlock'ta Accordion/Tab'ın çoklu panel/sekme grup yapısı modellenmedi (Task 11, bilinçli MVP sınırlaması).** Doküman (Madde 16.2) Accordion/Tab'ı blok tipi olarak sayıyor ama alt öğe (panel/sekme) modelini tanımlamıyor — her `PageContentBlock` satırı bağımsız bir içerik birimi; ardışık aynı-tipli blokların birlikte bir grup oluşturması yalnızca UI yorumu, zorunlu bir domain kuralı değil. Gerçek grup/panel yönetimi (GroupId, sıralı alt öğeler) gelecek fazda ayrıca ele alınmalı.
- **Docker Desktop bu makinede çalışır hale getirilemedi (Task 12, açık ortam kısıtı).** `Dockerfile`/`docker-compose.yml`/`.env.example` oluşturuldu ama `docker build`/`docker compose up` ile canlı doğrulama yapılamadı (`npipe` bağlantı hatası — Docker Desktop birden fazla kez başlatılmaya çalışıldı, engine hazır olmadı). Yerine, Docker'ın izleyeceği aynı kod yolu (config-driven migrate+seed) gerçek SQL Server'a karşı boş bir veritabanıyla iki kez çalıştırılarak eşdeğer şekilde doğrulandı (bkz. §6 Test Durumu). Docker Desktop çalışır hale geldiğinde canlı `docker compose up` doğrulaması yapılmalı — kod/config tarafında eksik kalan bir şey yok.
- **Yalnızca 5 modülün (Blog/Page/PageContentBlock/Product/News) unit testi var (Task 12+13, bilinçli kademeli "foundation" kapsamı).** Category/Collection/Document/ReferenceProject/Banner servisleri henüz test edilmedi — mevcut factory/mock/fixture altyapısı (`TESTING.md`) doğrudan tekrar kullanılarak genişletilebilir, yeni bir task olarak ele alınmalı.

**İleride dikkat edilmesi gereken mimari konular:**
- ~~Storage abstraction interface'i henüz yazılmadı (ADR-006)~~ — **TAMAMLANDI (Task 5.1, ADR-013; Task 6 ile ADR-014 ikinci kez doğruladı).** `IFileStorageService`/`LocalFileStorageService` iki modülde de sıfır değişiklikle çalıştı.
- 10.x paket sürüm tutarsızlığı (Serilog, Microsoft.Extensions.*.Abstractions) — ayrı küçük bir "paket hizalama" task'ı olarak bekliyor, acil değil. Task 5.1 ile `Microsoft.Extensions.Logging.Abstractions` (9.0.18) de Application.csproj'a eklendi — aynı ailede, ek bir tutarsızlık yaratmadı.
- Public site / SAP-CRM entegrasyon sınırı (ADR-002/009/010) — hiçbir modül tasarlanırken bu sınırın yanlışlıkla ihlal edilmemesine (public endpoint, SAP alanı vb. sızdırılmamasına) dikkat edilmeli.
- `ProductCode` değişikliğinde `ProductImage.FilePath`'in eski klasör adını koruması (Task 5.1, ADR-013'te "kabul edilen kozmetik risk" olarak belgeli) — veri kaybı yok, yalnızca izlenmesi gereken bir davranış notu.
- ~~`CategoryService`/`CollectionService.DeleteAsync`, Product FK referanslarını kontrol etmiyordu (çökme riski)~~ — **DÜZELTİLDİ (Task 6, 19.07.2026).** `IProductRepository.HasAnyWithCategoryIdAsync`/`HasAnyWithCollectionIdAsync` eklendi, her iki serviste kontrol ediliyor.

**Translation/Language modeliyle ilgili kararlar VE implementasyon — TAMAMEN KAPANDI (ADR-012 + Task 3.1B, 19.07.2026):**
- ~~`FieldName` sütununun kesin yapısı~~ → Düz `nvarchar`, kod tarafında const string. **Kodda:** `Translation.FieldName`, `nvarchar(100)`.
- ~~`Value` sütununun kesin tipi ve uzunluğu~~ → Tek `nvarchar(max)`. **Kodda:** uygulandı.
- ~~Her alan için ayrı kayıt mı, tek kayıtta mı~~ → Alan-bazlı ayrı satır. **Kodda:** uygulandı.
- ~~`EntityType`'ın temsili~~ → Enum + Infrastructure'da açık `ValueConverter` (`.ToString()`'e bağımlı değil). **Kodda:** `EntityTypeMapping`/`EntityTypeConverter`, bilinmeyen değerde `InvalidOperationException` doğrulandı.
- ~~Unique index ve constraint ayrıntıları~~ → `(EntityType, EntityId, LanguageId, FieldName)`. **Kodda:** `IX_Translations_Entity_Language_Field`, duplicate reddi sqlcmd ile doğrulandı.
- ~~Yetim (orphan) çeviri kaydı temizleme stratejisi~~ → Application katmanı, aynı transaction, trigger/job yok. **Kodda (Task 4.1, 19.07.2026):** `ITranslationService.DeleteTranslationsForAsync`, `CategoryService.DeleteAsync` içinde kullanılıyor, leaf kategori silindiğinde Translation temizliği doğrulandı — artık gerçek bir kullanım örneğiyle kanıtlı.
- ~~Ayrı bir `Language` entity'si mi, sabit liste/enum mu~~ → Ayrı `Language` entity'si (`Id`, `Code`, `Name`, `IsActive`, `DisplayOrder`, `Translations` navigation). **Kodda:** uygulandı, 7 dil seed edildi.
- ~~`Translation`↔`Language` ilişkisinin yönü~~ → `Translation.LanguageId`→`Language.Id` surrogate FK, `ON DELETE RESTRICT`. **Kodda:** doğrulandı (RU dili silme denemesi reddedildi, hiç silinmedi).
- ~~`Language.Code` normalizasyonu nerede yapılmalı~~ → Entity'de değil, Application katmanında (henüz bir Application servisi yok, Task 3.1B'nin kapsamı dışında — LanguageSeeder zaten normalize edilmiş sabit değerler kullanıyor).
- **Kalıcı yapısal risk (karar değil, kabul edilmiş sınırlama):** Polimorfik `EntityId` nedeniyle DB seviyesinde gerçek FK yok — bütünlük tamamen Application disiplinine bağlı, ADR-012'de belgeli.
- Çoklu dil fallback davranışı (TR göster/boş bırak/yayınlama) — ADR-007 ile bilinçli olarak "Gelecek Faz/Karar Bekleniyor" durumunda bırakıldı, bu public site'a ait bir konu, bu fazda tekrar açılmayacak. (Bu tek madde hâlâ açık — bilinçli olarak.)

**Kategori Yönetimi ile ilgili kararlar — TAMAMEN KAPANDI (Task 4.1, 18-19.07.2026):**
- Category native çevrilebilir sütun tutmayacak, tamamen Translation'a taşınacak → **Kodda:** uygulandı, `CategoryFields` sabitleri (Name/Description/SeoUrl/MetaTitle/MetaDescription).
- Hiyerarşi 2 seviyeyle sınırlı → **Kodda:** doğrulandı (3. seviye reddi test edildi).
- Silme davranışı (hard delete + alt-kategori kısıtı + Translation temizliği) → **Kodda:** doğrulandı.
- Yetim Translation temizliği tasarımı (`ITranslationService`, generic repository/CQRS/trigger yok) → **Kodda:** uygulandı ve doğrulandı.
- Application'ın persistence erişim biçimi (EF Core'suz, `ICategoryRepository`/`ITranslationService`/`IUnitOfWork`) → **Kodda:** uygulandı — projenin ilk Application katmanı emsali.
- Rol yetkisi (Admin/Ürün Yöneticisi/İçerik Editörü/SEO Editörü matrisi) → **Kodda:** `[Authorize(Roles=...)]` ile uygulandı, ama **canlı çoklu-rol testi yapılmadı** (yukarıdaki "Henüz doğrulanmamış noktalar"a bkz.).
- `CategorySeeder` bilinçli olarak oluşturulmadı — Madde 19.1'deki 13 kategori veri aktarımı/Excel import task'ında ele alınacak.

**Koleksiyon Yönetimi ile ilgili kararlar — TAMAMEN KAPANDI (Task 4.2, 19.07.2026):**
- **Collection↔Category ilişkisi:** Yok — 4 bağımsız doküman kanıtıyla (Madde 20, 36.1, 27.2, 18.1) netleşti, `CategoryId` eklenmedi. Bu, Task 4.1A'da işaretlenen olası çelişkinin kesin çözümü.
- Collection native çevrilebilir sütun tutmayacak → **Kodda:** uygulandı, `CollectionFields` sabitleri.
- Duplicate TR isim kontrolü **global** (Category ilişkisi olmadığı için "aynı parent altında" değil) → **Kodda:** doğrulandı.
- Silme davranışı (hard delete + Translation temizliği, Product/Document kontrolü henüz yok) → **Kodda:** doğrulandı.
- Mevcut `ITranslationService`/`IUnitOfWork` **hiç değiştirilmeden** tekrar kullanıldı — yeni genel-amaçlı abstraction eklenmedi.
- Rol yetkisi Category ile birebir aynı matris → **Kodda:** uygulandı, **canlı çoklu-rol testi yapılmadı** (yukarıya bkz.).
- `CollectionSeeder` bilinçli olarak oluşturulmadı — Madde 20'deki 272 gerçek seri adı veri aktarımı/Excel import task'ında ele alınacak.

**Ürün Yönetimi (çekirdek CRUD) ile ilgili kararlar — TAMAMEN KAPANDI (Task 5, 19.07.2026):**
- **SeriesName↔Collection ilişkisi:** `SeriesName`, `Product.CollectionId` FK'sine bağlandı; ayrı `CollectionName` metin alanı eklenmedi (Madde 20 + 272 seri örtüşmesiyle netleşti). `Category` da `CategoryId` FK'sine bağlandı — Product hem ikisine de bağımsız FK taşıyor.
- `Brand`/`Status` enum'ları `HasConversion<string>()` ile saklanıyor (EntityType'ın aksine polimorfik olmadıkları için ayrı `ValueConverter` sınıfı gerekmedi).
- `CreatedAt`/`UpdatedAt` yalnızca Product'ta var (Madde 18.1'in doküman-gerekçeli istisnası, Category/Collection'da yok).
- RBAC: Madde 30'un alan-seviyeli kısmi yetkileri **bilinçli olarak uygulanmadı** — action-seviyeli desen kullanıldı (yukarıdaki "Bilinen Riskler"e bkz.).
- `ProductSeeder` yok — 1.170 gerçek ürün verisi Excel Import task'ının (#17) konusu.

**Ürün Görselleri Yönetimi ile ilgili kararlar — TAMAMEN KAPANDI (Task 5.1, 19.07.2026):**
- **Product.ImagePath hiç var olmadığı için** (Task 5'te bilinçli eklenmemişti) `ProductImage` sıfırdan, migration riski olmadan eklendi.
- 5 görsel tipi (Render/Face/Lifestyle/Texture/Detail, Madde 18.2) → `ImageType` enum. Klasör yapısı Madde 35.4/ADR-006'ya literal sadakatle: `/uploads/products/{ProductCode}/{tip}/{guid}.{uzanti}`.
- Storage abstraction: `IFileStorageService` (Application, Stream-tabanlı) + `LocalFileStorageService` (Infrastructure) — **ADR-013** olarak kaydedildi, ADR-006'yı somutlaştırıyor.
- Ana görsel garantisi çift katmanlı: Application iş kuralı + DB filtered unique index (`WHERE IsPrimary=1`) — projenin ilk filtered unique index'i.
- Dosya+DB tutarlılığı telafi mantığıyla yönetiliyor (yükleme başarısızsa dosya geri silinir; silme başarısızsa hata loglanır ama DB kaynak doğruluğu kabul edilir) — tam atomiklik yok, bilinçli kabul edilen risk.
- RBAC: Category/Collection/Product ile aynı action-seviyeli desen, Task 5'in kararı sessizce genişletilmedi.

**Katalog/Doküman Yönetimi ile ilgili kararlar — TAMAMEN KAPANDI (Task 6, 19.07.2026):**
- **Çoklu dil:** Translation KULLANILMADI — `Document.Title` native sütun, `Document.LanguageId` satırın dilini belirtiyor (Madde 18.3'ün dosya-başına-tek-dil kanıtıyla netleşti). `EntityType.Document` eklenmedi.
- **İlişki modeli:** `Document`↔`Product`/`Collection` many-to-many (`ProductDocument`/`CollectionDocument` junction, Madde 36.1/36.2'de doğrudan yazılı), opsiyonel ("genel seviye" doküman mümkün, Madde 24).
- **Silme davranışı:** Product/Collection silme → yalnızca ilişki satırı (Cascade), Document+fiziksel dosya korunur.
- **Klasör yapısı:** ADR-006/Madde 35.4'ün literal `/products/{urunKodu}/...` örneğine M2M/opsiyonel cardinality nedeniyle uyulamadı, `/uploads/documents/{tip}/{guid}.pdf` kullanıldı — ADR-014 ile gerekçeli sapma olarak kaydedildi.
- **Storage:** Mevcut `IFileStorageService`/`LocalFileStorageService` (Task 5.1) hiç değiştirilmeden tekrar kullanıldı.
- **DocumentType:** Kapalı liste — Madde 24'ün kendi tablosundan (Catalog/TechnicalSheet/Certificate/Report), uydurulmadı.
- RBAC: Madde 30'un açık satırına literal sadakat — İçerik Editörü yalnızca Yükleme, SEO Editörü hiç erişim yok, Ürün Yöneticisi Tam (silme dahil).
- **Yan-düzeltme:** `CategoryService`/`CollectionService.DeleteAsync`'e Product-referans kontrolü eklendi (yukarıdaki "Bilinen Riskler"e bkz.).

**Referans Proje Yönetimi ile ilgili kararlar — TAMAMEN KAPANDI (Task 7, 19.07.2026):**
- **RBAC:** Madde 30 tablosunda satır yok — Madde 7.2'nin İçerik Editörü tanımına ("...referans proje...") dayanarak Blog/Haber/Banner'la aynı satır uygulandı: Admin=Tam, İçerik Editörü=CRUD (silme dahil), SEO Editörü=salt-görüntüleme, Ürün Yöneticisi=erişim yok.
- **İlişki modeli:** `ReferenceProject`↔`Product` many-to-many (`ProductReferenceProject` junction, Madde 23.1/36.2), tamamen opsiyonel; Restrict FK hiçbir yönde yok — Task 6'daki gibi bir silme-guard yan-bulgusu bu task'ta oluşmadı.
- **Görsel modeli:** ProductImage'ın 5-tipli modeli kullanılmadı — Madde 23.1 "Images: gallery" + "FeaturedImage: image" için tek `IsFeatured` bayrağıyla galeri+kapak tek tabloda (`ReferenceProjectImage`) birleştirildi.
- **Klasör yapısı:** `/uploads/projects/{ReferenceProjectId}/{guid}.{uzanti}` — doğal bir iş kodu (ProductCode gibi) olmadığı için surrogate Id kullanıldı (ADR-014'teki sapmanın aynısı).
- **Zorunluluk:** Madde 23.1 tablosunda Zorunluluk sütunu yok — yalnızca TR ProjectName + ProjectType zorunlu, geri kalan (Location/Architect/Year/görseller/ilişkiler) opsiyonel (Müşteri Notu'ndaki "boş arşiv" senaryosunu destekler).
- **Storage:** Mevcut `IFileStorageService`/`LocalFileStorageService` (Task 5.1) hiç değiştirilmeden üçüncü kez tekrar kullanıldı.
- Yeni ADR gerekmedi — ADR-006/013/014 deseni hiçbir yeni genel-amaçlı abstraction eklenmeden tekrar kullanıldı.

**Blog Yönetimi ile ilgili kararlar — TAMAMEN KAPANDI (Task 8, 19.07.2026):**
- **RBAC:** Madde 30 tablosunda Blog/Haber satırı literal mevcuttu (ReferenceProject'teki gibi çıkarım gerekmedi): Admin=Tam, İçerik Editörü=CRUD (silme dahil), SEO Editörü=salt-görüntüleme (Madde 30'un "Meta Alanları" ifadesi, projede hiç uygulanmayan alan-seviyeli RBAC yerine action-seviyeli view-only'e indirgendi), Ürün Yöneticisi=erişim yok. `BlogCategoryController` aynı matrisi kullanıyor (Madde 17.2 "kategori"yi Blog Yönetimi'nin fonksiyonu sayıyor, ayrı satır yok).
- **Kategori ilişkisi:** `BlogCategory` — Product'ın Kategori Yönetimi'nden tamamen ayrı, düz/hiyerarşisiz yeni bir entity (Madde 36.1 "BlogCategories" farklı tablo). Many-to-one, nullable FK + `SetNull` (Category/Collection'ın zorunlu-Restrict deseninden bilinçli farklı). Madde 28.2 "kategori adı"yı genel çoklu-dil alanları arasında saydığı için `BlogCategory.Name` Translation'a taşındı — yeni `EntityType.BlogCategory` eklendi (migration gerektirmedi, sadece enum+mapping).
- **Etiket ilişkisi:** `Tag`/`BlogTag` many-to-many, native (Tags alanı doküman'da "(multi-lang)" işaretli değil). Get-or-create + case-insensitive unique index ile paylaşılan/tekrar-kullanılabilir bir havuz — blog silindiğinde Tag satırları **silinmez** (bilinçli tasarım, doküman ayrı bir etiket yönetim ekranı istemiyor).
- **Görsel modeli:** Madde 21.1 "FeaturedImage: image" tekil (galeri değil) — ayrı bir `BlogImage` tablosu/entity'si oluşturulmadı, `Blog.FeaturedImagePath` doğrudan entity'de nullable string; ama yükleme/değiştirme/silme ProductImage ile birebir aynı güvenlik desenini (uzantı whitelist+MIME+magic-byte+GUID dosya adı+telafi mantığı) kullanıyor.
- **SEO:** Mevcut Translation-gömülü SeoUrl/MetaTitle/MetaDescription deseni korundu, Madde 36.1'in ayrı `SeoMeta` polimorfik tablosu kullanılmadı (SEO veri sözleşmesi #4'ün konusu).
- **Storage:** Mevcut `IFileStorageService`/`LocalFileStorageService` hiç değiştirilmeden dördüncü kez tekrar kullanıldı.
- Yeni ADR gerekmedi.

**Haber Yönetimi ile ilgili kararlar — TAMAMEN KAPANDI (Task 9, 19.07.2026):**
- **RBAC:** Madde 30'un "Blog/Haber" tek satırı — BlogController ile birebir aynı: Admin=Tam, İçerik Editörü=CRUD (silme dahil), SEO Editörü=salt-görüntüleme, Ürün Yöneticisi=erişim yok. `NewsCategoryController` aynı matrisi kullanıyor.
- **Alan sınıflandırması:** Madde 22'nin prose metni Blog'un 21.1 tablosundaki Excerpt/Author/Tags'i içermiyor — bu üçü Haber'e **kasıtlı olarak eklenmedi**, "Blog ile aynı deseni tekrar kullan" talimatı doküman-kanıtsız alanların kopyalanması için gerekçe sayılmadı.
- **Kategori ilişkisi:** `NewsCategory` — BlogCategory'nin birebir tekrarı (düz/hiyerarşisiz, Translation-tabanlı Name, yeni `EntityType.NewsCategory`). Many-to-one, nullable FK + `SetNull`. Madde 22'nin verdiği 6 sabit kategori adı bilinçli olarak seed edilmedi (CategorySeeder/CollectionSeeder emsali).
- **Status:** Doküman News için somut durum değerleri vermiyor — Blog'un örneği (Taslak/Yayında/Arşiv) esas alınarak ayrı, bağımsız bir `NewsStatus` enum'u oluşturuldu (BlogStatus ile paylaşılmadı — proje genelinde enum paylaşımı hiç yapılmıyor).
- **Görsel modeli:** Madde 22 "kapak görseli" tekil (galeri hiç anılmıyor) — Blog ile birebir aynı karar, ayrı `NewsImage` tablosu yok, `News.FeaturedImagePath` doğrudan entity'de.
- **SEO:** Blog ile birebir aynı Translation-gömülü desen.
- **Storage:** Mevcut `IFileStorageService`/`LocalFileStorageService` hiç değiştirilmeden beşinci kez tekrar kullanıldı.
- **Derleme notu:** `namespace Application.News` içindeki bare `News` kullanımı `Domain.Entities.News` ile ad çakıştığı için (CS0118) üç noktada tam nitelikli ada (`Domain.Entities.News`) geçildi — mimari etkisi yok.
- Yeni ADR gerekmedi.

**Banner Yönetimi ile ilgili kararlar — TAMAMEN KAPANDI (Task 10, 19.07.2026):**
- **RBAC:** Madde 30 Banner satırı — Blog/Haber'den farklı: Admin=Tam, İçerik Editörü=CRUD (silme dahil), SEO Editörü=— (salt-görüntüleme bile yok), Ürün Yöneticisi=—. Tek bir `ViewRoles`=`EditRoles` sabiti yeterli oldu.
- **Alan sınıflandırması:** Madde 16.1 Blog/News'ten üç somut noktada ayrılıyor — sıralama (DisplayOrder eklendi), 2-durumlu aktif/pasif (bool IsActive+ToggleActive, Status enum değil), yayın tarihi **aralığı** (Start+End, tekil PublishDate değil). SEO alanları hiç eklenmedi (Madde 17.2/36.1 Banner için SEO anmıyor).
- **İlişki yok:** Banner hiçbir başka entity'ye bağlı değil (Category/Tag/Product yok) — projenin en yalın CMS modülü.
- **Görsel modeli:** Doküman "görsel/video yükleme" diyor; video desteği mevcut doğrulama altyapısının (magic-byte/MIME whitelist yalnızca görsel formatları için) kapsamı dışında bırakıldı — yalnızca görsel desteklendi, `Banner.ImagePath` tekil alan (Blog/News ile aynı desen).
- **BannerType eklenmedi:** Madde 17.2'nin "Hero banner, kampanya banner, anasayfa bileşenleri" örnekleyici ifadesi bir tip taksonomisi vermiyor — icat edilmedi, tek düz `Banner` entity'si yeterli.
- **Storage:** Mevcut `IFileStorageService`/`LocalFileStorageService` hiç değiştirilmeden altıncı kez tekrar kullanıldı.
- **EntityType:** Yeni üye eklenmedi — `EntityType.Banner` Task 3.1B'den beri enum'da rezerveydi, bu task'ta ilk kez gerçek kullanıma girdi.
- Yeni ADR gerekmedi.

**Testing Foundation ile ilgili kararlar — TAMAMEN KAPANDI (Task 12, 19.07.2026):**
- **Test DB stratejisi:** SQLite in-memory, **`UseInMemoryDatabase` reddedildi** — InMemory sağlayıcı FK/unique/cascade davranışını uygulamıyor, yanlış-pozitif riski taşıyor. `SqliteCompatibleModelCustomizer` ile üretim modelinin iki SQLite-uyumsuz noktası (nvarchar(max), case-sensitivity) yalnızca test modelinde adapte edildi, üretim koduna dokunulmadı.
- **Unit test felsefesi:** "Sociable unit test" — gerçek DB-backed repository + UnitOfWork (EF-atanan Id'ler gerçek), yalnızca dosya I/O ve Translation persistence sahte. Saf Moq izolasyonu, `private set` Id'li entity'lerin two-phase create akışı nedeniyle pratik değildi.
- **Integration test kimlik doğrulama:** Gerçek cookie/login akışı değil, `TestAuthHandler` (`X-Test-Role` header) — yalnızca "kim giriş yapmış" sahte, `[Authorize(Roles=...)]` middleware'i ve **AntiForgery tamamen gerçek** kalıyor.
- **Migration/Index audit sonucu:** 12 migration incelendi, hiçbiri geriye dönük değiştirilmedi. 2 gerçek eksik composite index bulundu (`PageContentBlocks`, `ProductImages` — ikisi de gerçek `.OrderBy` sorgu kanıtıyla), tek corrective migration (`AddPerformanceAndConstraintIndexes`) ile eklendi. Spekülatif index'ler (Product.Status/Brand, Blog/News Status+PublishDate) **eklenmedi** — sorgu kanıtı yok.
- **Startup migration/seed politikası:** `DatabaseInitialization:ApplyMigrationsOnStartup`/`SeedOnStartup` (varsayılan `false`/`true`) — ADR-004'ün açık bıraktığı "production'da migration nasıl tetiklenecek" sorusunun config-driven çözümü. Yeni ADR gerekmedi, ADR-004'ü tamamlıyor.
- **Docker:** `Dockerfile`+`docker-compose.yml`+`.env.example` oluşturuldu, **canlı doğrulama bu makinede yapılamadı** (Docker Desktop çalışmadı — açık ortam kısıtı, bkz. §9).
- **Page↔PageContentBlock ilişkisi:** Doğrulandı — gerçekten bire-çok (`Page 1 --- N PageContentBlock`), kod/migration hiç yanlış değildi. Yalnızca PROGRESS.md'deki "1:1-sahipli" ifadesi belirsizdi, düzeltildi.
- **Coverage kapsamı:** Yalnızca Blog/Page/PageContentBlock/Product servisleri + 2 seeder hedeflendi (bilinçli "foundation, exhaustive değil" kararı) — global %11.5 line coverage bu yüzden düşük, kalan 10+ modül henüz test edilmedi.
- Yeni ADR gerekmedi.

**Bayi/Showroom Yönetimi ile ilgili kararlar — TAMAMEN KAPANDI (Task 14, 19.07.2026):**
- **Modelleme:** Tek `Dealer` entity + nullable `Category` enum (ADR-008, `Dealer=2`/`Showroom=3` — doküman/legacy veri kodlarıyla birebir eşleşiyor). Ayrı bir `Showroom` ana entity'si veya ayrı bir panel modülü **oluşturulmadı**.
- **Kategorisiz (17) kayıt:** `Category` **nullable** — yeni bir "Unclassified" enum üyesi icat edilmedi, mevcut nullable-FK deseniyle (BlogCategoryId?/NewsCategoryId?) tutarlı.
- **Translation:** **Kullanılmıyor** — Madde 25.1'in veri modeli tablosu hiçbir alanı "(multi-lang)" işaretlemiyor (Product/Blog/Proje'nin aksine). Projenin Translation altyapısını hiç tüketmeyen ilk CMS modülü; `EntityType.Dealer` (Task 3.1B'den beri rezerve) hâlâ tüketilmedi.
- **Görsel/galeri/açıklama/çalışma saatleri/randevu formu/sıralama:** **Eklenmedi** — Madde 25.1'in gerçek veri modeli tablosunda yoklar, yalnızca Madde 26'nın public-site anlatımında ("eklenebilir", "Karar Bekleniyor") geçiyorlar; ADR-008 bunu zaten "otomatik zorunlu değil" olarak işaretlemişti. `IFileStorageService` bu modülde **hiç kullanılmadı**.
- **SEO:** Eklenmedi — Madde 25.1'de SeoUrl/MetaTitle/MetaDescription yok; Madde 27.2'nin Bayi URL deseni (`/satis-noktalari/{il}/{ilce}`) City+District'ten programatik türetiliyor, per-kayıt özel alan gerektirmiyor.
- **Koordinat modeli:** `Latitude`/`Longitude` nullable `decimal(9,6)` (float/double kullanılmadı), service seviyesinde -90..90/-180..180 aralık + "ikisi birlikte veya hiçbiri" kuralı doğrulanıyor. Harita entegrasyonu (Google Maps vb.) eklenmedi — yalnızca koordinat verisi yönetiliyor, harita public-site'ın konusu.
- **RBAC:** Madde 30 satırı literal — Admin=Tam, İçerik Editörü=—, SEO Editörü=—, Ürün Yöneticisi=— — **projedeki ilk salt-Admin modül** (diğer tüm modüllerde en az bir rol salt-görüntüleme yetkisine sahipti). Tek `[Authorize(Roles = ApplicationRoles.Admin)]` yeterli, ayrı ViewRoles/EditRoles ayrımı yok.
- **Index review:** `DealerRepository` hiçbir SQL-seviyesi `.Where`/`.OrderBy` içermiyor (Index'teki Category/City filtresi in-memory) — yeni index eklenmedi, mevcut PK yeterli.
- **Excel Import / SAP / CRM:** Bu task'ta geliştirilmedi (backlog #17'nin ve Faz 2'nin konusu) — ama `DealerId` (int, otomatik artan) doğal bir entegrasyon anahtarı olarak kullanılabilir durumda bırakıldı.
- 212 gerçek kayıt **seed edilmedi** — yalnızca şema/enum kuruldu, gerçek veri aktarımı Excel Import'un konusu.
- Yeni ADR gerekmedi — mevcut ADR-008 güncellendi (3 açık nokta kesinleşti), yeni bir mimari karar değil.

**Form Yönetimi ile ilgili kararlar — TAMAMEN KAPANDI (Task 15, 20.07.2026):**
- **Modelleme:** Tek `FormSubmission` entity + `FormType` enum discriminator (Contact/RequestInformation/SampleRequest) — Madde 29'un somut alan listesiyle tanımladığı yalnızca 3 form türü. Dinamik form builder (`FormDefinition`/`FormField`/JSON blob) **kurulmadı** — doküman hiçbir yerde admin form tasarım ekranı tarif etmiyor.
- **Durum:** Status enum yerine `IsRead`/`ReadAt`/`ProcessedAt` nullable zaman damgaları — Madde 17.2 somut değer listesi vermiyor, Banner'ın nullable-zaman-damgası deseniyle tutarlı.
- **Translation:** Kullanılmadı — Dealer'dan sonra Translation'ı hiç tüketmeyen ikinci modül. Tüm alanlar kullanıcı tarafından girilen ham veri.
- **Dosya eki:** Yok — Madde 29.1/29.2/29.3'ün hiçbirinde dosya/CV alanı yok, `IFileStorageService` kullanılmadı.
- **Public form gönderimi / e-posta bildirimi:** Bu fazda kurulmadı — ADR-001/002/009 gereği public site kodu bu fazda hiç yazılmıyor. `FormSubmissionService.CreateSubmissionAsync` Application katmanında hazır ve test edilmiş, ama hiçbir `[AllowAnonymous]` controller'dan çağrılmıyor. SMTP/MailKit altyapısı eklenmedi.
- **RBAC:** Madde 30 satırı literal — Admin=Tam, İçerik Editörü=Görüntüleme, SEO Editörü=—, Ürün Yöneticisi=—. Details GET action'ında bilinçli olarak otomatik okundu-işaretleme yapılmadı (GET idempotent kalmalı + İçerik Editörü'nün dolaylı yazma tetiklememesi için).
- **Pagination/Index (ADR-015, projedeki ilk gerçek SQL-seviyesi pagination):** `GetPagedAsync` gerçek `IQueryable.Where/OrderByDescending/Skip/Take` + `CountAsync` kullanıyor — önceki tüm modüllerin `GetAllAsync()`+in-memory-filtreleme deseninden bilinçli sapma (form kayıtları sürekli büyüyen bir veri seti). 2 index eklendi: `CreatedAt` (her Index sayfası bununla sıralanıyor) ve `FormType+CreatedAt` (tip filtresi gerçek sorgu deseni). IsRead/Email/Phone'a index eklenmedi (düşük kardinalite/LIKE-arama sınırlı fayda).
- **Kişisel veri güvenliği:** Liste ekranında Message/AdminNote gösterilmiyor (yalnızca Details'te); Razor otomatik HTML-encode ile XSS'e karşı korunuyor, ayrı sanitize kütüphanesi eklenmedi; e-posta regex + telefon/mesaj/not maksimum uzunluk doğrulaması serviste.
- 212 gerçek form verisi seed edilmedi (zaten dokümanda böyle bir gereksinim yok — form kayıtları kullanıcı üretimli, seed edilecek "referans veri" değil).
- Yeni ADR eklendi (**ADR-015**) — tek tablo+discriminator kararı ve projedeki ilk gerçek pagination deseninin gelecekteki modüller için ilke olarak kaydı.

**Kullanıcı Yönetimi ile ilgili kararlar — TAMAMEN KAPANDI (Task 16 analiz + Task 16B implementasyon, 20.07.2026):**
- **Aktif/Pasif:** Yeni `ApplicationUser.IsActive` (bool, varsayılan true) — Identity'nin Lockout mekanizmasından (başarısız-girişe özel, süre-sınırlı) bilinçli olarak ayrı. Login akışında (`AccountController.Login`) `PasswordSignInAsync`'ten önce kontrol ediliyor.
- **Parola:** Admin doğrudan formda belirler (`CreateAsync(user,password)`) veya sıfırlar (`GeneratePasswordResetTokenAsync`+`ResetPasswordAsync`, token e-postayla gönderilmiyor, sunucu içinde üretilip aynı istekte tüketiliyor) — proje genelinde SMTP/e-posta altyapısı yok.
- **Silme:** Hard-delete izinli — kendi-hesap ve son-aktif-Admin guardrail'leriyle birlikte, projenin genel hard-delete deseniyle (Category/Collection/Blog vb.) tutarlı.
- **Mimari — `IUserManagementService` neden interface:** Application katmanı Infrastructure'a referans veremiyor ama implementasyon `UserManager<ApplicationUser>`/`RoleManager<IdentityRole>`'a bağımlı — arayüz Application'da (`Application/Users/`), implementasyon Infrastructure'da (`Infrastructure/Identity/UserManagementService.cs`). Katman sınırının zorunlu kıldığı istisna (Task 17'de `IRoleManagementService` ile aynı desen ikinci kez kullanıldı, bkz. aşağıdaki "Role Management ile ilgili kararlar").
- **Guardrail'ler:** `IsLastActiveAdminAsync` tek doğruluk kaynağı (Delete/Deactivate/rol-kaldırma hepsi kullanıyor); kendi-hesap kontrolü üç tehlikeli işlemde ayrı ayrı; **servis katmanında** uygulanıyor (yalnızca UI'da değil, controller bypass edilse bile korunuyor).
- **Rol ataması (REVİZE — 20.07.2026, implementasyon başlamadan önce):** `ApplicationRoles.All` whitelist — serbest metin/yeni rol kabul edilmiyor. Başlangıçta çoklu rol (checkbox listesi) planlanmıştı; implementasyona geçmeden önce kullanıcı bu kararı **tek rol** olarak değiştirdi (roller birbirini dışlayan iş rolleri, MVP'de tek rol yeterli) — Create/Edit formlarında dropdown/tek-seçim, `CreateUserRequest.Role`/`UpdateUserRequest.Role`/`UserDto.Role` tekil `string`. `AspNetUserRoles` tablosu/Identity mimarisi değişmedi, kural yalnızca `UserManagementService` seviyesinde uygulanıyor.
- **Email/UserName immutable (REVİZE — 20.07.2026, implementasyon başlamadan önce):** Başlangıçta Edit formunda Email düzenlenebilir planlanmıştı; kullanıcı bu kararı da implementasyondan önce değiştirdi — Email/UserName oluşturulduktan sonra hiçbir zaman değişmez (login bilgisi sabit tutulur, SecurityStamp/normalize karmaşıklığından kaçınılır). `UpdateUserRequest`'te `Email` alanı yok; Edit formunda Email salt-okunur metin.
- **RBAC:** Madde 30 satırı literal — Admin=Tam, diğer 3 rol=— (Dealer'daki salt-Admin desenle aynı, tek `[Authorize(Roles=Admin)]`).
- **Kapsam dışı (bilinçli):** Role Management ekranının dinamik rol tanımlama/silme kısmı (bkz. aşağıda Task 17), claim/permission altyapısı, alan-seviyeli RBAC, 2FA, SMTP/e-posta gönderimi, self-registration, public "şifremi unuttum" akışı, profil fotoğrafı, ad-soyad/departman gibi dokümanda olmayan alanlar, çoklu rol desteği (kaldırıldı).
- Migration `AddIsActiveToApplicationUser` — yalnızca `AspNetUsers.IsActive` kolonu; mevcut seed edilmiş admin/dev-test kullanıcıları migration sonrası `IsActive=1` (sqlcmd ile doğrulandı). Tek-rol/immutable-email revizyonu yeni bir migration gerektirmedi (AspNetUserRoles ve AspNetUsers.Email/UserName kolonları zaten mevcuttu, sadece uygulama katmanı kuralı değişti).
- Yeni ADR gerekmedi (mimari karar zaten ADR-005'in "Identity altyapısı sarmalanmayacak" ilkesinin doğal bir uygulaması); tek-rol/immutable-email revizyonu da mevcut ADR'leri etkilemedi.

**Role Management ile ilgili kararlar — TAMAMEN KAPANDI (Task 17 analiz + otomatik implementasyon, 20.07.2026):**
- **Kapsam:** Sabit 4 rol için salt-okunur denetim ekranı (Seçenek A). Doküman Madde 17.2'de "Kullanıcı/Rol Yönetimi" tek satır (#14) — ayrı bir "Role Management" modülü yok; Madde 7.2 dört rolü kapalı liste olarak tanımlıyor. Mevcut RBAC tamamen derleme-zamanı `[Authorize(Roles=...)]` sabitlerine dayandığından yeni bir rol otomatik erişim kazanamaz — bu, dinamik rol CRUD'ı hem dokümansız hem işlevsiz kılıyor, kritik bir mimari çelişki oluşturmadığı için implementasyona otomatik devam edildi (kullanıcıya soru sorulmadı).
- **`RoleController`:** Yalnızca `Index`/`Details` GET action'ları — hiçbir state-changing action yok (rol oluşturma/silme/yeniden adlandırma/kullanıcıya rol atama bu modülün konusu değil, ikincisi zaten User Management'ta var). AntiForgery/PRG gerekmiyor.
- **Yetki matrisi:** `RoleManagementService.PermissionMatrix` — her controller'ın gerçek `ViewRoles`/`EditRoles` sabitlerinden elle çıkarılmış statik veri; dinamik/reflection-tabanlı otomatik keşif kasıtlı olarak yapılmadı. Henüz controller'ı olmayan 3 modül (Dil/SEO/Excel Import) "Henüz Uygulanmadı" işaretli, tahmini erişim seviyesi verilmedi.
- **`ApplicationRoles.All` tek doğruluk kaynağı:** DB'de fazladan/beklenmeyen bir rol olsa bile (`_roleManager.Roles` DOĞRUDAN enumerate edilmiyor) yalnızca bu 4 rol sistem rolü olarak gösteriliyor.
- **Mimari — `IRoleManagementService` neden interface:** `IUserManagementService` (Task 16B, ADR-016) ile birebir aynı gerekçe — implementasyon `RoleManager<IdentityRole>`/`UserManager<ApplicationUser>`'a bağımlı. Bu, deseni ikinci kez uygulayarak "projedeki tek örnek" olma durumunu sona erdirdi; ADR-016'ya bu genişlemeyi kaydeden bir güncelleme notu eklendi.
- **Guardrail gerekmedi:** Hiçbir mutasyon action'ı olmadığı için son-aktif-Admin/kendi-hesap korumasına ihtiyaç yok (bu korumalar zaten User Management'ta, `UserManagementService.IsLastActiveAdminAsync`'te var — Role Management yalnızca görüntülüyor).
- **RBAC:** Madde 30 satırı literal — Admin=Tam, diğer 3 rol=— (`[Authorize(Roles=Admin)]`, aynı desen). Navigation linki (`_Layout.cshtml`) projedeki **ilk** rol-koşullu nav linki — diğer tüm linkler yalnızca `IsAuthenticated` şartına bağlı, bu ise ayrıca `User.IsInRole(ApplicationRoles.Admin)` kontrolü de yapıyor (görev talimatının açık isteği).
- **Kapsam dışı (bilinçli):** Dinamik rol oluşturma/silme/yeniden adlandırma, permission entity, claim CRUD, alan-seviyeli yetkilendirme, bu ekrandan kullanıcıya rol atama (User Management'ın konusu), Identity şema değişikliği.
- Migration gerekmedi — Identity şemasına hiç dokunulmadı, yetki matrisi kodda statik veri.
- Yeni ADR eklenmedi (ADR-016'nın ikinci kullanımı, güncelleme notu yeterli — yeni bir karar değil, mevcut kararın doğrulanması).

**Dashboard ile ilgili kararlar — TAMAMEN KAPANDI (Task 18 analiz + otomatik implementasyon, 20.07.2026):**
- **Kapsam:** Doküman Madde 17.2'de Dashboard yalnızca isim olarak geçiyor (#1) — kart/grafik/tablo tanımı yok. Kapsam tamamen mevcut 6 entity'nin gerçek alanlarından (kod taranarak, tahmin edilmeden) türetildi. Kritik bir mimari çelişki oluşturmadığı için implementasyona otomatik devam edildi.
- **Kartlar:** Ürün (Toplam/Aktif — `Status==Active`, `IsActive` yok), Kategori, Koleksiyon, Bayi/Showroom (`Dealer.Category` enum ayrımı), Kullanıcı (Toplam/Aktif), Form Başvurusu (Toplam), Bekleyen Formlar (Okunmamış+İşlenmemiş — `IsRead` ve `ProcessedAt` **bağımsız iki alan**, tek "Pending" alanına indirgenmedi çünkü bilgi kaybı yaratırdı).
- **Son kayıtlar:** Son 5 Ürün (ProductCode/Durum/Tarih — Translation-tabanlı çevrilebilir isim kullanılmadı, ITranslationService toplu okuma sağlamıyor, 5 kayıt için N+1'den kaçınıldı), Son 5 Form Başvurusu.
- **"Son eklenen kullanıcılar" desteklenmiyor:** `ApplicationUser`'da `CreatedAt` yok (`IdentityUser` temel sınıfı sağlamıyor, Task 16B'de de eklenmedi) — icat edilmedi, eklenmedi.
- **Mimari — `DashboardService` neden `AppDbContext`'e doğrudan bağımlı:** Mevcut 6 modül repository'sinin hiçbirinde `CountAsync`/predicate yok (hepsi `GetAllAsync()` tam liste). Bunlara yeni metod eklemek (scope creep) yerine `DashboardService` (Infrastructure) doğrudan `AppDbContext` kullanıyor — ADR-016'nın Identity-özel olmayan, genel bir uygulanışı; ADR-016'ya bu genişlemeyi kaydeden bir not eklendi.
- **Performans:** Tüm sorgular `AsNoTracking()` + DB-seviyesi `CountAsync()`/`Where().CountAsync()`/`OrderByDescending().Take(5)` — hiçbir tam tablo belleğe çekilmedi, N+1 yok (Recent Products/Forms projeksiyonları navigation property kullanmıyor).
- **RBAC:** Dört rol de aynı kartları görür (dokümanda/kodda farklılaşma kanıtı yok) — **ancak** "Son Form Başvuruları" bölümü yalnızca `FormSubmissionController.ViewRoles`'e (Admin+İçerik Editörü) sahip roller için render ediliyor (SeoEditor/ProductManager'ın forma hiç erişimi yok, PII/link-güvenliği ihlali olmasın diye — `Product/Index.cshtml`'deki `canEdit` deseniyle aynı yaklaşım, "farklı dashboard icat etme" değil).
- **Tasarım dili düzeltmesi:** Projede hiç CSS framework kurulu değil (`wwwroot` yok) — 17 önceki task boyunca çıplak semantik HTML kullanılmış, Dashboard da aynı konvansiyonu izledi.
- **Kapsam dışı (bilinçli):** Audit/activity log, gerçek-zamanlı Dashboard, SignalR, grafik paketi, yapay grafik verisi, yeni entity/tablo/migration, export, bildirim merkezi, AJAX otomatik yenileme.
- Migration gerekmedi — yalnızca mevcut verileri okuyor, yeni entity/tablo yok.
- Yeni ADR eklenmedi (ADR-016'nın ikinci genişlemesi, güncelleme notu yeterli).

---

# 10. Son Durum Özeti

1. Proje: NG Kütahya Seramik Admin Panel — yalnızca Admin Panel + backend, public site bu fazda yok.
2. Task 0 (Analiz + 11 ADR), Task 1 (Identity Foundation), Task 2 (Authentication + RBAC) **tamamen kapalı**.
3. Solution: 4 katman (Presentation/Application/Domain/Infrastructure), net9.0, build 0/0.
4. Identity: ASP.NET Core Identity + Cookie, JWT yok, 4 rol (`ApplicationRoles` const string).
5. Veritabanı: `.\SQLEXPRESS` / `NGKutahyaSeramikAdminPanel`, `InitialIdentity` (7 Identity tablosu) + `AddTranslationInfrastructure` (`Languages`, `Translations`) + `AddCategories` (`Categories`) + `AddCollections` (`Collections`) + `AddProducts` (`Products`) + `AddProductImages` (`ProductImages`) + `AddDocuments` (`Documents`, `ProductDocuments`, `CollectionDocuments`) + `AddReferenceProjects` (`ReferenceProjects`, `ReferenceProjectImages`, `ProductReferenceProjects`) + `AddBlog` (`Blogs`, `BlogCategories`, `Tags`, `BlogTags`) + `AddNews` (`News`, `NewsCategories`) + `AddBanner` (`Banners`) + `AddPages` (`Pages`, `PageContentBlocks`) migration'ları uygulandı.
6. Kullanıcılar: `admin@localhost` (Admin), `editor@localhost` (İçerik Editörü, development-only) — parolalar yalnızca User Secrets'ta.
7. Login/Logout/AccessDenied/Dashboard uçtan uca çalışıyor; ReturnUrl + open redirect koruması doğrulandı.
8. Rol-bazlı yetkilendirme doğrudan `[Authorize(Roles=...)]` ile çalışıyor; `HomeController.AdminOnly()` organik AccessDenied ile doğrulandı; Category, Collection, Product, ProductImage, Document, ReferenceProject, Blog/BlogCategory, News/NewsCategory, Banner ve Page/PageContentBlock modüllerinde de aynı temel desen kullanıldı (canlı çoklu-rol testi bekliyor; Product/ProductImage/Page için Madde 30'un alan-seviyeli kısmi yetki isteği bilinçli olarak uygulanmadı — bu artık iki bağımsız modülde tekrarlanan bir teknik borç, yeni backlog maddesi #23'e bağlandı; Document/ReferenceProject/Blog/News için Madde 30'un açık/dolaylı RBAC ipucuna literal sadakatle uyuldu, "Meta Alanları" ifadesi tutarlı biçimde salt-görüntüleme'ye indirgendi; Banner'da ise SEO Editörü'ne hiç erişim verilmedi (SEO alanı yok) — bkz. madde 9 "Bilinen Riskler").
9. `AppDbContext` artık `Languages`/`Translations`/`Categories`/`Collections`/`Products`/`ProductImages`/`Documents`/`ProductDocuments`/`CollectionDocuments`/`ReferenceProjects`/`ReferenceProjectImages`/`ProductReferenceProjects`/`Blogs`/`BlogCategories`/`Tags`/`BlogTags`/`News`/`NewsCategories`/`Banners`/`Pages`/`PageContentBlocks` `DbSet`'lerini içeriyor — Domain'in ilk entity'leri (`Language`,`Translation`, Task 3.1B) ve on üç CRUD entity'si (`Category` Task 4.1, `Collection` Task 4.2 — ikisi FK ile birbirine bağlı değil; `Product` Task 5 — hem `CategoryId` hem `CollectionId`'ye bağımsız FK taşıyor; `ProductImage` Task 5.1 — Product'a Cascade FK, projenin ilk çocuk-entity'si ve ilk gerçek dosya yükleme özelliği; `Document` Task 6 — Product'a/Collection'a doğrudan FK yok, `ProductDocument`/`CollectionDocument` junction'ları üzerinden M2M; `ReferenceProject` Task 7 — Product'a doğrudan FK yok, `ProductReferenceProject` junction'ı üzerinden M2M, kendi galeri+kapak görseli `ReferenceProjectImage`; `Blog`/`BlogCategory`/`Tag` Task 8 — Blog'un tekil kapak görseli ayrı tabloya değil doğrudan entity'ye yazılıyor, `Blog`↔`Tag` M2M (`BlogTag`), `Blog`↔`BlogCategory` nullable FK+SetNull; `News`/`NewsCategory` Task 9 — Blog'un tekil-kapak-görseli+nullable-FK-SetNull desenlerinin birebir tekrarı, ama M2M ilişkisi yok; `Banner` Task 10 — hiçbir ilişkisi olmayan, en yalın entity, tekil görsel+bool IsActive+tarih aralığı; `Page`/`PageContentBlock` Task 11 — Page'in projede ilk kez hiçbir native alanı yok (yalnızca CreatedAt/UpdatedAt), PageContentBlock Page'e Cascade FK ile bağlı, 5 blok tipini tek entity'de birleştiriyor).
10. **Task 3.1A/3.1B (Translation/Language), Task 4.1 (Kategori), Task 4.2 (Koleksiyon), Task 5 (Ürün, çekirdek CRUD), Task 5.1 (Ürün Görselleri), Task 6 (Katalog/Doküman), Task 7 (Referans Proje), Task 8 (Blog), Task 9 (Haber), Task 10 (Banner) ve Task 11 (Sayfa) TAMAMLANDI.** 7 dil seed edildi, Category için 10/10, Collection için 6/6, Product için 20/20, ProductImage için 34/34, Document için 54/54, ReferenceProject için 38/38, Blog için 35/35, News için 31/31, Banner için 33/33, Page/PageContentBlock için 45/45 iş kuralı/güvenlik senaryosu doğrulandı, build 0/0. Açık mimari karar kalmadı (yalnızca alan-seviyeli RBAC ve Accordion/Tab grup yönetimi teknik borç olarak açık).
11. **Task 4.1'den itibaren çalışma düzeni değişti:** plan+implementasyon+migration+doğrulama+dokümantasyon artık tek task içinde birlikte yürütülüyor — Task 4.2, Task 5, Task 5.1, Task 6, Task 7, Task 8, Task 9, Task 10 ve Task 11 bu düzenin başarılı uygulamaları.
12. **Application katmanı artık on modülün gerçek kodunu içeriyor** (`CategoryService`+`CollectionService`+`ProductService`+`ProductImageService`+`DocumentService`+`ReferenceProjectService`+`ReferenceProjectImageService`+`BlogService`+`BlogCategoryService`+`NewsService`+`NewsCategoryService`+`BannerService`+`PageService`+`PageContentBlockService`, `ICategoryRepository`+`ICollectionRepository`+`IProductRepository`+`IProductImageRepository`+`IDocumentRepository`+`IReferenceProjectRepository`+`IReferenceProjectImageRepository`+`IBlogRepository`+`IBlogCategoryRepository`+`ITagRepository`+`INewsRepository`+`INewsCategoryRepository`+`IBannerRepository`+`IPageRepository`+`IPageContentBlockRepository`, ortak `ITranslationService`/`IUnitOfWork`) — EF Core'a hiç referans vermiyor; `IFileStorageService` artık yedi farklı modülde (ProductImage, Document, ReferenceProject, Blog, News, Banner, Page) sıfır değişiklikle çalıştı.
13. `Database.MigrateAsync()` hiçbir zaman kullanılmıyor; migration/database update her zaman CLI ile manuel.
14. Named policy, generic repository, CQRS, BaseEntity, soft delete, SPA, Razor Pages, Identity UI scaffold — hiçbiri kullanılmıyor/kullanılmayacak. Audit alanları (CreatedAt/UpdatedAt) Category/Collection/ProductImage/Document/ReferenceProject/Blog/News/Banner'da yok ama **Product'ta doküman-gerekçeli istisna olarak var** (Madde 18.1); **Page/PageContentBlock'ta da var** (kullanıcı talimatıyla, doküman-gerekçeli değil — bu farkla kayıtlı).
15. Gerçek credential'lar (parolalar) hiçbir dosyada/logda/raporda yer almıyor — yalnızca User Secrets'ta. Dil verisi sır değil, doğrudan seeder kodunda; kategori/koleksiyon/ürün/görsel/doküman/blog/haber/banner/sayfa verisi henüz seed edilmedi (bilinçli olarak — gerçek veri aktarımı Excel Import task'ının konusu; Madde 22'nin 6 sabit haber kategorisi de bilinçli olarak seed edilmedi).
16. **Dosya depolama artık yedi modülde kanıtlanmış (ADR-013 + ADR-014):** `IFileStorageService`/`LocalFileStorageService` hem tek-sahipli entity'lerde (`ProductImage` → `wwwroot/uploads/products/{ProductCode}/{tip}/{guid}.{uzanti}`, `ReferenceProjectImage` → `wwwroot/uploads/projects/{ReferenceProjectId}/{guid}.{uzanti}`, `Blog.FeaturedImagePath` → `wwwroot/uploads/blog/{BlogId}/{guid}.{uzanti}`, `News.FeaturedImagePath` → `wwwroot/uploads/news/{NewsId}/{guid}.{uzanti}`, `Banner.ImagePath` → `wwwroot/uploads/banners/{BannerId}/{guid}.{uzanti}`, `PageContentBlock.ImagePath` → `wwwroot/uploads/pages/{PageId}/{guid}.{uzanti}` — hepsi tekil alan, ayrı galeri tablosu yok) hem M2M/opsiyonel-ilişkili entity'lerde (`Document` → `wwwroot/uploads/documents/{tip}/{guid}.pdf`) sıfır değişiklikle çalışıyor.
17. **Task 6 sırasında Category/Collection'da gerçek bir çökme hatası bulunup düzeltildi:** `Product.CategoryId`/`CollectionId` (Task 5'in Restrict FK'leri) referans kontrolü olmadan silme denenirse uygulama çöküyordu — artık anlaşılır hata mesajıyla reddediliyor. Task 7/8/9/10/11'de benzer bir risk hiç oluşmadı (ReferenceProject↔Product tamamen Cascade/M2M; Blog/News↔Category nullable FK+SetNull; Banner'ın hiçbir ilişkisi yok; Page↔PageContentBlock bire-çok/tek-sahipli Cascade — Restrict FK hiçbirinde yok).
18. **Task 11 ile projede ilk kez "blok tipi değişiminde çapraz-alan otomatik temizliği" kuralı uygulandı:** `PageContentBlock` tipi görsel-kullanmayan bir tipe değiştirildiğinde eski görsel (DB+disk) otomatik silinir; VideoEmbed'den başka bir tipe geçildiğinde `VideoEmbedUrl` otomatik temizlenir. Bu davranış doğrulama senaryolarında (12-13 numaralı testler) bizzat test edildi.
19. **Task 12 (Testing Foundation) TAMAMLANDI (19.07.2026) — yeni bir CMS modülü değil, projenin ilk kalıcı otomatik test altyapısı:** `tests/NGKutahyaSeramik.UnitTests` (51 test) + `tests/NGKutahyaSeramik.IntegrationTests` (37 test), toplam 88/88 ✅, SQLite in-memory ile gerçek ilişkisel test (`UseInMemoryDatabase` hiç kullanılmadı), `WebApplicationFactory<Program>` ile gerçek HTTP/RBAC (5 istemci tipi)/AntiForgery/PRG testleri, model factory'ler, seeder testleri, `DatabaseInitialization:ApplyMigrationsOnStartup`/`SeedOnStartup` config-driven başlangıç politikası (ADR-004'ün açık bıraktığı sorunun çözümü, yeni ADR gerekmedi), Docker altyapısı oluşturuldu (canlı doğrulama bu makinede Docker Desktop çalışmadığı için yapılamadı — açık ortam kısıtı), 12 migration'ın tam denetimi + 1 corrective migration (`AddPerformanceAndConstraintIndexes`), Page↔PageContentBlock ilişkisinin gerçekten bire-çok olduğu doğrulandı (kod hiç yanlış değildi, yalnızca dokümantasyon dili düzeltildi), `TESTING.md` oluşturuldu. Coverage: global %11.5 line (bilinçli — yalnızca 4 modül hedeflendi), hedeflenen servislerde %75-93.
20. **Task 13 (Haber Yönetimi Test Sertleştirmesi) TAMAMLANDI (19.07.2026) — News/NewsCategory modülü Task 9'da zaten tam kuruluydu, bu task yalnızca test kapsamını genişletti:** Prompt News'i sıfırdan kurmayı tarif ediyordu ama gerçek kod incelemesi zaten var olduğunu gösterdi — kullanıcıya soruldu, "mevcut modülü test standardına yükselt" onaylandı. `NewsFactory`/`NewsCategoryFactory` + 24 yeni unit test (`NewsServiceTests` 16, `NewsCategoryServiceTests` 8) + 19 yeni integration test (Anonymous 7, RBAC 8, AntiForgery/PRG 3, Relational 1) eklendi. Domain/Application/Infrastructure/Presentation/migration'a **hiç dokunulmadı**. Index/SEO review yapıldı — News repository'lerinde SQL-seviyesi `.Where`/`.OrderBy` olmadığı doğrulandı, yeni index/migration gerekmedi (Task 12'nin kararı yeniden teyit edildi). Solution toplamı: **131/131 test** (75 unit + 56 integration), coverage %14.1 line'a yükseldi (`NewsService` %80.5, `NewsCategoryService` %88.6). Docker'a dokunulmadı (talimatla açıkça yasaklandı, final stabilization task'ına bırakıldı).
21. **Task 14 (Bayi/Showroom Yönetimi) TAMAMLANDI (19.07.2026)** — backlog #11, gerçekten yeni bir modül. Tek `Dealer` entity + nullable `DealerCategory` enum (`Dealer=2`/`Showroom=3`, doküman kodlarıyla birebir — 17 kategorisiz kayıt gerçeğini nullable ile yansıtıyor). ADR-008'in açık bıraktığı 3 nokta bu task'ta kesinleşti: Translation kullanılmıyor (Madde 25.1'in hiçbir alanı multi-lang değil — projenin Translation'ı hiç tüketmeyen ilk modülü), görsel/galeri/açıklama/çalışma-saatleri/randevu-formu eklenmedi (yalnızca public-site anlatımında geçiyorlar), SEO alanı yok (Bayi URL'i City+District'ten programatik türetiliyor, per-kayıt slug değil). RBAC Madde 30 satırı literal: Admin=Tam, diğer 3 rol=— (projedeki ilk salt-Admin modül). 165/165 test (92 unit + 73 integration), `DealerService` %93.4 line coverage, migration `AddDealers` yalnızca `Dealers` tablosunu içeriyor.
22. **Task 15 (Form Yönetimi) TAMAMLANDI (20.07.2026)** — backlog #16, gerçekten yeni bir modül. Tek `FormSubmission` entity + `FormType` discriminator (Contact/RequestInformation/SampleRequest — Madde 29'un somut alan listesiyle tanımladığı yalnızca 3 tür, dinamik form builder yok). Status enum yerine IsRead/ReadAt/ProcessedAt nullable-zaman-damgası (doküman somut değer vermiyor). Translation'ı hiç tüketmeyen ikinci modül (Dealer'dan sonra), dosya eki yok. Public form gönderim endpoint'i ve e-posta bildirimi ADR-001/002/009 gereği bu fazda kurulmadı — yalnızca admin veri modeli/servis/UI. **ADR-015 eklendi — projedeki ilk gerçek SQL-seviyesi pagination/filtreleme deseni** (`GetPagedAsync`, `IQueryable.Where/OrderByDescending/Skip/Take`+`CountAsync`, tüm önceki modüllerin in-memory listeleme deseninden bilinçli sapma) + bundan sonraki modüller için "ne zaman pagination" ilkesi. RBAC Madde 30 satırı literal: Admin=Tam, İçerik Editörü=Görüntüleme, diğer ikisi=—. 202/202 test (116 unit + 86 integration), `FormSubmissionService` %85.2/`FormSubmissionRepository` %92.6 line coverage, migration `AddFormSubmissions` yalnızca `FormSubmissions` tablosunu + 2 index içeriyor.
23. **Task 16/16B (Kullanıcı Yönetimi) TAMAMLANDI (20.07.2026)** — backlog #2'nin CRUD/RBAC kısmı kapandı. Önce analiz-only Task 16 (30 başlıklı rapor, kod değişikliği yok) 3 kritik belirsizliği (`IsActive` vs Lockout, parola belirleme, silme davranışı) `AskUserQuestion` ile netleştirdi; Task 16B o kararlarla implemente etti. `ApplicationUser.IsActive` (yeni alan, migration `AddIsActiveToApplicationUser` — yalnızca `AspNetUsers` kolonu), `IUserManagementService` (projedeki **ilk** interface'li servis — Application/Infrastructure katman sınırının zorunlu kıldığı istisna), hard-delete + kendi-hesap + son-aktif-Admin guardrail'leri **servis katmanında**. RBAC Madde 30 satırı literal: Admin=Tam, diğer 3 rol=—. 269/269 test (148 unit + 121 integration), `UserManagementService` %88.5 line coverage.
24. **Task 17 (Role Management) TAMAMLANDI (20.07.2026)** — backlog #2'nin geri kalan kısmı kapandı (dinamik rol CRUD hariç — dokümanda hiç dayanağı olmadığı için kapsama hiç alınmadı, "başlanmamış" değil). Analiz, kritik bir mimari/kapsam çelişkisi bulmadığı için aynı oturumda otomatik implementasyona geçti. Sabit 4 rol için salt-okunur `RoleController` (Index/Details GET-only, state-changing action yok, AntiForgery/PRG gerekmiyor). Yetki matrisi, her controller'ın gerçek `ViewRoles`/`EditRoles` sabitlerinden elle çıkarılmış statik veri (`RoleManagementService.PermissionMatrix`) — dinamik/reflection-tabanlı keşif kasıtlı olarak yapılmadı; henüz controller'ı olmayan 3 modül "Henüz Uygulanmadı" işaretli. `IRoleManagementService` — ADR-016 deseninin **ikinci** kullanımı (artık iki interface'li servis var, ADR-016 güncellendi). `ApplicationRoles.All` tek doğruluk kaynağı. 297/297 test (163 unit + 134 integration), `RoleManagementService`/`RoleController` %100 line coverage. Migration gerekmedi.
25. **Task 18 (Dashboard) TAMAMLANDI (20.07.2026)** — Madde 17.2 modül #1, dokümanda kart/grafik tanımı yok. Analiz, kritik bir çelişki bulmadığı için aynı oturumda otomatik implementasyona geçti. Önceden tamamen boş olan `HomeController.Index` artık gerçek veriye bağlı: 8 özet kartı (Ürün Toplam/Aktif, Kategori, Koleksiyon, Bayi, Showroom, Kullanıcı Toplam/Aktif, Form Toplam, Bekleyen Formlar [Okunmamış+İşlenmemiş — `IsRead`/`ProcessedAt` bağımsız iki alan]) + Son 5 Ürün + Son 5 Form Başvurusu. `DashboardService` (Infrastructure) doğrudan `AppDbContext`'e bağımlı — ADR-016'nın Identity-özel olmayan genel bir uygulanışı (mevcut 6 modül repository'sine dokunulmadı). Dört rol de aynı kartları görür; "Son Form Başvuruları" bölümü yalnızca `FormSubmissionController.ViewRoles`'e sahip roller için render ediliyor (link-güvenliği). 326/326 test (178 unit + 148 integration), Dashboard kodu %100 line coverage. Migration/yeni paket yok.
26. Bir sonraki oturum: Bu dosyayı okuyup, hangi modülle (SEO veri sözleşmesi #4, alan-seviyeli RBAC #23, Excel Import, Docker Desktop çalışır hale geldiğinde canlı `docker compose up` doğrulaması, kalan modüllerin unit testlerinin genişletilmesi, veya public site fazında Form Yönetimi'nin gönderim endpoint'i+e-posta bildirimi) devam edileceğini kullanıcıya sor — TASKS.md'deki backlog sıralaması öneri, henüz onaylanmadı. Ürün Yönetimi'nin çekirdek CRUD'ı (Task 5), görsel yönetimi (Task 5.1), doküman ilişkisi (Task 6), referans proje ilişkisi (Task 7), Blog (Task 8), Haber (Task 9), Banner (Task 10), Sayfa (Task 11) Yönetimi, Testing Foundation (Task 12), Haber Test Sertleştirmesi (Task 13), Bayi/Showroom Yönetimi (Task 14), Form Yönetimi (Task 15), Kullanıcı Yönetimi (Task 16/16B), Role Management (Task 17) ve Dashboard (Task 18) bitti — yalnızca Excel Import (#17) hâlâ ayrı bir backlog maddesi. Referans PDF `Downloads` klasöründe.
