# PROJECT MEMORY — NG Kütahya Seramik Yönetim Paneli

> Bu dosya ASLA sıfırlanmaz. Her task sonunda güncellenir, eski bilgiler korunur; yanlış/değişen kayıtlar silinmez, "Düzeltildi" iziyle işaretlenir.

---

## Kapsam Tanımı (Kesinleşti — 17.07.2026)

**Genel doküman kapsamı:**
Public web sitesi + Yönetim paneli.

**Mevcut geliştirme ve teslimat kapsamı:**
Yönetim paneli + Backend API.

**Gelecek faz:**
Public web sitesi. Şu anda public site kodu ve public endpoint geliştirilmeyecek.

---

## Projenin Mevcut Durumu

- Referans doküman: `NG_Kutahya_Seramik_Kavramsal_Analiz_v2.pdf` (v2.0, 06.07.2026, 58 sayfa, Most Idea Yazılım).
- Yukarıdaki kapsam tanımı geçerlidir.
- Solution ve katmanlı mimari iskeleti oluşturuldu (Task 1.1, 17.07.2026).
- Identity ve DbContext altyapısı kuruldu (Task 1.2A, 17.07.2026) — ApplicationUser, ApplicationRoles, AppDbContext, Infrastructure DI kayıtları, Cookie konfigürasyonu.
- İlk migration oluşturuldu ve development veritabanına uygulandı, 4 rol seed edildi (Task 1.2B, 18.07.2026).
- İlk development admin kullanıcısı, kullanıcı tarafından sağlanan gerçek credential ile User Secrets üzerinden seed edildi ve idempotency doğrulandı (Task 1.2C, 18.07.2026).
- Authentication UI (Login/Logout/AccessDenied + korumalı Dashboard) uçtan uca çalışır durumda kuruldu ve doğrulandı (Task 2.1, 18.07.2026).
- Rol-bazlı authorization (`[Authorize(Roles=...)]`) doğrudan kullanımıyla kuruldu; development-only ikinci test kullanıcısı (İçerik Editörü rolü) seed edildi; organik AccessDenied senaryosu gerçek bir yetkisiz erişimle doğrulandı (Task 2.2, 18.07.2026). Henüz domain entity/business logic/CRUD yok.

## Bulunduğumuz Faz

**Faz 1 — Yönetim Paneli + Backend API** (uygulama başladı). Faz 0 — Analiz tamamlandı.

## Identity Foundation Completed (18.07.2026)

**Task 1 (Identity Foundation) tamamen kapatılmıştır.** Kapsadığı alt task'lar: 1.1 (Solution ve Katman İskeleti), 1.2A (Identity ve DbContext Altyapısı), 1.2B (İlk Migration, Veritabanı Oluşturma ve Identity Seed), 1.2C (İlk Admin Kullanıcısının Seed Edilmesi ve İdempotency Doğrulaması). Kapanış kontrolü: `AspNetUsers`'daki admin kullanıcısının Email/UserName/EmailConfirmed alanları ve `AspNetUserRoles`⋈`AspNetRoles` join'i ile **yalnızca Admin rolüne** sahip olduğu (başka rol yok, toplam rol sayısı=1) doğrulandı — dosya değişikliği/migration/seed çalıştırılmadan, salt okunur SQL sorgularıyla. Sonraki adım: Task 2.1 (Authentication UI) planlama aşaması.

## Bulunduğumuz Task

**Backlog Kapanışı — Alan-Seviyeli RBAC + Dil Yönetimi + SEO + Excel Import (Task 20-23) — TAMAMLANDI (21.07.2026).**

Kullanıcı beş maddelik kapsamlı bir talimat verdi: (1) mevcut durum/kod denetimi, (2) alan-seviyeli RBAC, (3) Dil Yönetimi panel modülü, (4) SEO veri sözleşmesi+Yönetimi, (5) Excel Import — "belgeler ile kod arasında fark varsa tahmin yapma, raporla" ve "net değilse dur ve sor" şartıyla. Faz 1 analizi gerçek kod taranarak yapıldı (migration listesi/AppDbContext/RBAC sabitleri/PermissionMatrix/mevcut test klasörü/`dotnet build+test` baseline — 326/326), doküman-kod arasında **hiçbir çelişki bulunmadı** (Language/SEO/Excel Import kodda gerçekten yoktu, docs ile birebir tutarlı). İki gerçek belirsizlik (SEO URL tekillik kapsamı, Excel şablonu ile mevcut Product validasyonu arasındaki alan uyuşmazlığı) `AskUserQuestion` ile kullanıcıya soruldu ve onaylanan tercihlerle ilerlendi — bkz. ilgili alt bölümler. **Faz sonunda solution toplamı 326 → 402 test (204 unit + 198 integration).**

**Faz 2 — Alan-seviyeli RBAC (backlog #23):** `ProductController`/`PageController`'ın `EditRoles`'u önceden İçerik Editörü/SEO Editörü'nü tamamen dışlıyordu (yalnızca ViewRoles'taydılar, hiç düzenleyemiyorlardı) — bu iki role artık **Edit** action'ına erişim açıldı (`FieldEditRoles`), ama `RestrictToPermittedFields` (Controller katmanında, DB'den taze okunan mevcut değerle izinsiz alanı geri yazan overposting-korumalı mantık) ile hangi alanları değiştirebilecekleri sıkı sıkıya sınırlandı: İçerik Editörü→Name/ShortDescription/LongDescription, SEO Editörü→SeoUrl/MetaTitle/MetaDescription (ikisi de Product'ta), SEO Editörü→SeoUrl/MetaTitle/MetaDescription (Page'de, Title hariç). Create action'ları bilinçli olarak değişmedi (bu roller yeni kayıt oluşturamaz). `RoleAccessLevel.PartialFields` yeni enum üyesi eklendi, `RoleManagementService.PermissionMatrix` güncellendi. View'da da (fieldset/input `disabled`) UI-seviyesi gizleme var ama backend kontrolü asıl güvenlik sınırı. 14 yeni test.

**Faz 3 — Dil Yönetimi panel modülü (backlog #3):** Language/Translation ALTYAPISI zaten Task 3.1A/3.1B'de vardı (ADR-012) — yalnızca CRUD ekranı eksikti. `/Language` Index (7 dil) + güvenli Edit (Code salt okunur — View'da disabled+hidden ikili input deseni; Name/DisplayOrder serbest; TR'nin "Aktif" kutusu her zaman kilitli — `LanguageService.UpdateAsync` içinde `Code=="TR" && !IsActive` reddi). Create/Delete action'ı **hiç yok** — "seed edilmiş diller silinemez" kuralı, silinecek bir action olmamasıyla otomatik sağlanıyor. Admin-only. 22 yeni test.

**Faz 4 — SEO veri sözleşmesi + SEO Yönetimi (backlog #4):** Kritik bulgu — kullanıcının "8 modülde SEO alanı var" varsayımı **kodla çelişiyordu**: `BannerFields.cs`'te SEO alanı **hiç yok** (Task 10'da zaten bilinçli olarak eklenmemişti), `ReferenceProjectFields.cs`'te yalnızca SeoUrl var, MetaTitle/MetaDescription **yok**. Gerçek kapsam: Product/Category/Collection/Blog/News/Page (tam 3 alan) + ReferenceProject (yalnızca SeoUrl) = 7 modül. `SeoManagementService`, bu 7 modülün zaten var olan servislerini (kayıt varlığı+görünen ad için) + `ITranslationService`'i (asıl okuma/yazma için) sarmalayan ince bir cross-cutting katman — **ikinci bir SEO tablosu oluşturulmadı**, mevcut Translation mimarisi tek doğruluk kaynağı kaldı. SEO URL tekilliği kullanıcı onayıyla **EntityType+Dil bazında, DB constraint değil uygulama-seviyesi rapor/uyarı** (`SeoUrlNormalizer` — Türkçe karakter duyarlı slug normalizasyonu). RBAC: Admin+SEO Editörü tam (bu MERKEZİ ekrana; diğer roller kendi modüllerinde Faz 2'nin alan-seviyeli RBAC'ı üzerinden zaten erişiyor, burada tekrar erişim verilmedi). **Bulunan gerçek bug:** İlk implementasyonda `SeoManagementService.UpdateAsync` `IUnitOfWork.SaveChangesAsync()` çağırmıyordu (Translation değişiklikleri DbContext'te tracked kalıp hiç commit edilmiyordu) — yalnızca gerçek HTTP+DB'ye giden integration testlerde ortaya çıktı (unit test yazılsaydı muhtemelen fake/mock ile maskelenirdi), düzeltildi. Public sitemap/robots.txt/redirect/schema **oluşturulmadı** (ADR-001/002/009, kapsam dışı). 24 yeni test (12 unit + 12 integration).

**Faz 5 — Excel Import (backlog #17, yalnızca Ürün Yönetimi):** Referans dokümanı (`Downloads/NG_Kutahya_Seramik_Kavramsal_Analiz_v2.pdf`, `pdftotext` ile okundu) Madde 37.2'nin **tam 11 sütununu** birebir veriyor: ProductCode/ProductName/Series/Status/Size/Surface/Color (zorunlu) + Category/UsageArea/ApplicationArea/FaceCount (opsiyonel) — dokümanda olmayan hiçbir sütun icat edilmedi. **Kritik çelişki (kullanıcıya soruldu, onaylandı):** Bu 11 sütun, mevcut `ProductService.ValidateAsync`'in zorunlu tuttuğu Unit/Thickness/BodyType/TR-ShortDescription'ı hiç karşılamıyor — kullanıcı "Excel için bu alanları geçici olarak zorunlu olmaktan çıkar" dedi (manuel form hiç değişmedi). Bu yüzden `ProductImportService`, `ProductService.CreateAsync/UpdateAsync`'i (ve onun sıkı validasyonunu) **çağırmıyor** — `IProductRepository`/`ITranslationService` üzerinden `Product` entity'sini doğrudan oluşturuyor/güncelliyor (Domain'in kendisinde hiç invariant-check olmadığı doğrulandıktan sonra), yalnızca Madde 37.2'den türetilmiş kendi (daha dar) kurallarını uyguluyor. Category/Collection (Series) bulunamazsa **otomatik oluşturulmuyor** (satır reddediliyor). Akış: Şablon indir (ClosedXML, MIT — `dotnet add package`, THIRD_PARTY_NOTICES.md güncellendi) → Yükle (uzantı+MIME+magic-byte `50 4B 03 04`+10MB boyut kontrolü) → Ön izleme (satır bazlı hata, DB yazması yok) → Onayla (aynı dosya **yeniden** parse/validate edilir — önizlemedeki client verisine güvenilmez — tek `IUnitOfWork.BeginTransactionAsync` transaction'ı içinde yazılır, beklenmeyen hata tüm batch'i geri alır) → Sonuç raporu (PRG, TempData ile 6 sayaç). Geçici dosya `IFileStorageService` (Task 5.1'den, `OpenReadAsync` yeni eklendi — additive) ile `product-imports/` altında saklanıp işlem sonunda (`finally` bloğunda, başarı/hata fark etmeksizin) silindi. `IUnitOfWork`'e `BeginTransactionAsync`/`IUnitOfWorkTransaction` eklendi (additive, `FakeUnitOfWork` da güncellendi). RBAC: Admin+Ürün Yöneticisi. 14 integration test (geçerli/eksik-header/duplicate-ProductCode/bulunamayan-Series-otomatik-oluşturmuyor/bozuk-dosya/yanlış-uzantı/tam-akış-DB-yazması-ve-geçici-dosya-temizliği/AntiForgery/RBAC/PRG dahil).

**Migration:** Hiçbirinde yok — Language/Translation/Product şemaları zaten mevcuttu, SEO Translation'ın üzerine kuruldu, Excel Import da mevcut Product tablosunu kullanıyor.

**Doğrulama:** Her fazdan sonra `dotnet build` (0/0) + `dotnet test` ayrı ayrı çalıştırıldı, tarayıcıda uçtan uca manuel doğrulama yapıldı (Language Edit TR-guardrail, SEO Yönetimi 7 tip seçici+raporlar, Excel Import şablon indirme gerçek ZIP imzasıyla doğrulandı). Final: **402/402 test** (204 unit + 198 integration).

**Takip — Excel Import: Madde 37.3 Import Sonrası Raporlama (Task 24, 21.07.2026).** Kullanıcı Excel Import'u yeniden incelettirdi — Madde 37.3'ün istediği "hatalı kayıt/eksik zorunlu alan/pasif ürün/yeni-güncellenen özeti" raporları ilk sürümde eksikti (yalnızca 6 sayaç TempData'da tutuluyordu, satır-seviyesi detay Confirm sonrası kayboluyordu). **Var olan import pipeline'ına dokunmadan** (kesin talimat) yalnızca yeni bir raporlama katmanı eklendi:
- `ProductImportResultDto`'ya tek bir additive alan (`TransactionRolledBack`) eklendi — `ImportAsync`'in zaten var olan try/catch'inin transaction sonucunu dışarı açması için (rollback olduğunda parse-zamanında IsValid=true görünen satırların bile GERÇEKTE yazılmadığını rapora doğru yansıtmak amacıyla; aksi hâlde rollback sonrası bile "Yeni/Güncellendi" görünürlerdi).
- Yeni, saf/DB'siz `ProductImportReportBuilder` (sınıflandırma: Created/Updated/Failed/Skipped + "zorunludur." son ekiyle eksik-zorunlu-alan tespiti + Status=Pasif tespiti) ve `ProductImportCsvWriter` (server-side CSV, OWASP CSV injection koruması — `=,+,-,@` ile başlayan hücre tek tırnakla metne zorlanır, standart virgül/tırnak/satır-sonu escaping'i ayrıca uygulanır).
- Rapor, `IFileStorageService` üzerinden (Task 5.1'den, hiç değişmeden) JSON olarak `product-import-reports/` altına kalıcı kaydediliyor — geçici Excel dosyasının aksine (o hâlâ işlem sonunda siliniyor) rapor dosyası silinmiyor, "panelden görüntülenebilir" gereksinimi kalıcılık istiyor. **Yeni migration yok** — ikinci bir DB tablosu değil, dosya tabanlı.
- `/ProductImport/Result` artık `reportPath` query param'ıyla (Confirm'ün PRG redirect'inde taşınıyor, TempData'ya bağımlı değil — sayfa yenilense bile rapor kaybolmuyor) sekmeli (Tümü/Hatalı/Eksik Zorunlu Alan/Pasif Ürün/Yeni Eklenen/Güncellenen) bir tablo gösteriyor; `/ProductImport/DownloadReportCsv?reportPath=...` CSV indiriyor. Eşleşmeyen görsel raporu **bilinçli olarak uygulanmadı** (Excel Import'ta hiç görsel eşleştirme yok, Madde 37.4 dokümanın kendisi "Karar Bekleniyor" diyor — icat edilmedi, teknik borç olarak kayıtlı).
- RBAC hiç değişmedi (Admin+Ürün Yöneticisi, sınıf-seviyesi `[Authorize]` zaten kapsıyor).
- **Doğrulama:** 18 yeni unit test (`ProductImportReportBuilderTests` — sınıflandırma/rollback senaryosu dahil; `ProductImportCsvWriterTests` — 4 formül-tetikleyici karakter + escaping) + 6 yeni integration test (karma sonuç raporu, 0-geçerli-satır bile indirilebilir rapor üretiyor, CSV injection uçtan uca, RBAC). Solution toplamı 402 → **426/426 test** (222 unit + 204 integration). Clean build 0/0.

---

**Önceki durum (Tabler UI Entegrasyonu ve takipleri) korunuyor, bkz. aşağıdaki "Önceki Task Detayı".**

## Önceki Task Detayı — Tabler UI Entegrasyonu — Faz 1 Prototip — TAMAMLANDI (20.07.2026).

Yalnızca Presentation/UI katmanını kapsayan, backend'e (Controller/Service/Repository/Entity/Migration/RBAC) hiç dokunmayan bir görsel yenileme. Tabler (MIT, https://github.com/tabler/tabler) admin template'i **yerel** olarak (`wwwroot/lib/`, CDN yok) projeye eklendi, sabit sürümlerle: `@@tabler/core@@1.4.0` (npm'den `npm pack` ile indirildi, `tabler.min.css`/`tabler.min.js` alındı) + `@@tabler/icons-webfont@@3.45.0` (`tabler-icons.min.css` + woff2/woff/ttf font dosyaları). Her ikisi de doğrulanmış MIT lisanslı — tam lisans metinleri `THIRD_PARTY_NOTICES.md`'de. `pnpm` build sistemi, Liquid şablonları, demo backend/veri, önizleme uygulaması ve React/Vue/Angular sürümleri hiç alınmadı — yalnızca derlenmiş CSS/JS/font dosyaları.

**Önemli düzeltme (talimat-vs-gerçeklik):** Talimat "mevcut Bootstrap" tabanlı bir template öngörüyordu ama Task 18'in kaydettiği gibi projede o ana kadar **hiç CSS framework yoktu** (çıplak semantik HTML). Bu, Tabler'ın kendisinin zaten Bootstrap tabanlı olması sayesinde çelişki yaratmadı — Tabler kurulumuyla birlikte proje ilk kez bir CSS framework kazandı, önceki "framework yok" durumu artık geçerli değil (bu task'ın kendisi tam olarak bunu değiştirmek için vardı).

**Marka teması:** Tabler'ın varsayılan mavi paleti kullanılmadı — `wwwroot/css/site.css`, Tabler'ın `--tblr-primary`/`--tblr-body-bg`/`--tblr-bg-surface`/`--tblr-border-color` token'larını NG'nin kendi CSS değişkenleriyle (`--ng-bg`, `--ng-sidebar` #202522, `--ng-accent` #a48968 bronz vb.) override ediyor — Tabler'ın kendi CSS dosyasına hiç dokunulmadı, yalnızca cascade ile üzerine yazıldı (sonradan yüklenen kazanır). Sidebar'ın koyu teması (`data-bs-theme="dark"`) için Tabler'ın dark-mode token cascade'ine güvenmek yerine `.navbar-vertical[data-bs-theme="dark"]` seçicisiyle doğrudan hedefli override yapıldı — piksel-kesin marka rengi garantisi için.

**Layout değişikliği:** Üst yatay menü tamamen kaldırıldı; `_Sidebar.cshtml` (koyu, `navbar-vertical`, 5 grup: Genel/Ürün Yönetimi/İçerik/Operasyon/Sistem) + `_Topbar.cshtml` (sol: sayfa başlığı `ViewData["Title"]`, sağ: e-posta+rol rozeti+çıkış) yeni partial'lar olarak eklendi. Login için ayrı, sidebar'sız `_LoginLayout.cshtml` oluşturuldu (Tabler'ın `page-center` auth deseni).

**Menü görünürlüğü — gerçek RBAC'a bire bir sadık:** Sidebar'daki her öğe, ilgili controller'ın **gerçek** `ViewRoles` sabitinden (kod taranarak, tahmin edilmeden) `User.IsInRole(...)` kontrolüyle koşullu render ediliyor — ör. Kategori/Koleksiyon/Ürün yalnızca Admin+ProductManager+ContentEditor+SeoEditor'a, Bayi/Showroom ve Kullanıcı/Rol yalnızca Admin'e görünüyor. Bu yalnızca UI kozmetiği — backend authorization (`[Authorize(Roles=...)]`) hiç değişmedi, tek doğruluk kaynağı olmaya devam ediyor.

**Prototip kapsamı (talimatın kendi sınırı):** Yalnızca `_Layout.cshtml`, Sidebar, Topbar, Login, Dashboard, Product Index/Create/Edit, User Index yenilendi. Diğer 9 modül (Category/Collection/Document/ReferenceProject/Blog/News/Banner/Page/Dealer/FormSubmission/Role) henüz eski çıplak HTML'i kullanıyor — yeni Tabler layout'un içinde render olacaklar ama kendi içerikleri henüz reskin edilmedi (bilinçli, aynı oturumda tüm proje değiştirilmedi).

**Kesin kısıtlara sadakat:** Controller/Service/Repository/Entity/Migration/route/form action/input name/model binding/AntiForgery/Validation/RBAC/testler **hiç değişmedi** — yalnızca `.cshtml` dosyaları ve yeni `wwwroot` varlıkları eklendi/değiştirildi. `AntiForgeryHelper`'ın `__RequestVerificationToken` regex'i, ASP.NET'in `asp-*` tag helper'larının otomatik ürettiği hidden input'a hâlâ bağımlı — bu hiç dokunulmadığı için sorunsuz çalışıyor.

**Doğrulama:** Clean build 0/0 (bir önceki manuel `dotnet run` sürecinin `bin/Presentation.exe`'yi kilitli tutması nedeniyle alternatif çıktı klasörüyle doğrulandı — kod/Razor derleme hatası değildi). Solution toplamı **326/326 test** değişmeden geçti (görsel değişiklik hiçbir test metnini/route'unu/RBAC davranışını bozmadı). Uygulama gerçek SQL Server verisiyle manuel olarak çalıştırıldı: Login (POST→302→Dashboard), Logout (POST→302→Login), Dashboard (gerçek sayaçlar+boş durumlar), Ürün Yönetimi (boş durum+CTA), Ürün Ekle formu (7 dil çeviri alanı+antiforgery token doğrulandı), Kullanıcı Yönetimi (gerçek 2 kullanıcı listelendi) — hepsi konsol hatası olmadan, doğru sidebar/topbar ile render edildi.

**Sonraki adım:** Kullanıcının onayı sonrası aynı design system'in kalan 9 modüle (Category/Collection/Document/ReferenceProject/Blog/News/Banner/Page/Dealer/FormSubmission/Role Index/Create/Edit ekranları) yaygınlaştırılması.

**Takip #1 — Ürün formunda çeviri alanları Tabs'e taşındı (20.07.2026).** `_ProductForm.cshtml`'deki "Çeviriler" kartı, dilleri alt alta dizen `<fieldset>` bloklarından Tabler/Bootstrap `nav-tabs`+`tab-content`'e çevrildi — 7 dil artık ayrı sekmede, TR varsayılan aktif ve "zorunlu" rozetini koruyor. `asp-for` binding'leri, hidden `LanguageId`/`LanguageCode`/`LanguageName` alanları ve `Translations[i]` indeksleme mantığı birebir korundu — yalnızca görsel kapsayıcı değişti. ("Hatalı sekmeyi otomatik öne çıkar" fikri denendi ama gereksiz bulunup kaldırıldı: doğrulama hataları `ModelState`'e alan-bazlı değil `ModelState.AddModelError(string.Empty, ...)` ile özet olarak ekleniyor — TR sekmesi zaten varsayılan aktif olduğu için ek mantığa gerek yoktu.) 326/326 test değişmeden geçti.

**Takip #2 — Seed edilen admin e-postası admin@localhost → admin@localhost.com (20.07.2026).** `SeedAdmin:Email` yalnızca User Secrets'ta tutulan bir config değeri (kaynak kodda hiç hardcoded değildi) — `dotnet user-secrets set` ile güncellendi. Config'i değiştirmek tek başına yetersizdi: `IdentitySeeder.SeedAdminUserAsync`, `FindByEmailAsync(yeni-email)` bulamayınca **ikinci bir admin kullanıcı oluştururdu** (eski `admin@localhost` satırı DB'de kalıp duplicate admin doğardı). Bunun yerine mevcut satır güvenli şekilde **yerinde yeniden adlandırıldı**: `Program.cs`'e geçici, bir-kereye-mahsus bir blok eklenip (`UserManager.SetEmailAsync`+`SetUserNameAsync`, ham SQL değil — Identity'nin kendi normalizer'ı Email/UserName/NormalizedEmail/NormalizedUserName'i tutarlı hesapladı) tek seferlik `dotnet run` ile çalıştırıldı, sqlcmd ile tek satır+doğru normalized alanlar+korunan rol ataması+değişmeyen PasswordHash doğrulandı, ardından blok kaynaktan tamamen kaldırıldı (Task 1.2C'den beri süregelen "geçici doğrulama kodu sonradan silinir" konvansiyonu). Login ekranındaki Email input'una `placeholder="admin@localhost.com"` eklendi (önceden hiç placeholder yoktu). Test tarafında değiştirilecek bir şey **çıkmadı** — `UserManagementTests.cs`'teki gerçek login-akışı testleri (`Login_ActiveUser_WithCorrectPassword_Succeeds` vb.) tamamen sentetik `*@test.local` adresleri kullanıyor, gerçek seed edilen admin adresine hiç bağımlı değil (doğrulandı, grep ile teyit edildi). 326/326 test değişmeden geçti; tarayıcıda yeni adresle gerçek giriş doğrulandı.

**Takip #3 — Kategori ve Koleksiyon ekranları Tabler'a taşındı (20.07.2026).** Kullanıcı ekran görüntüsüyle bildirdi: `/Category/Create` ve `/Collection/Create` hâlâ çıplak tarayıcı-varsayılanı HTML render ediyordu (bu 9 modülün Faz 1 prototip kapsamı dışında bırakıldığı zaten biliniyordu, ama kullanıcı bu ikisini öncelikli görmek istedi). `_CategoryForm.cshtml`/`_CollectionForm.cshtml` Product formundaki desenle (card+`row g-3`+`form-control`/`form-select`+dil-başına-sekme `nav-tabs`) birebir aynı şekilde yenilendi; `Create.cshtml`/`Edit.cshtml` `ViewData["Title"]`+"Listeye Dön" linkini aldı. Category Index'in iç içe (parent/child) ağaç görünümü `<ul>/<li>`'den Tabler `list-group`+nested-`list-group`'a (girinti için `ms-4 border-start ps-3`) çevrildi — metin/rota/rol mantığı birebir korundu. Collection Index düz tablo yapısı Product Index'teki `table-responsive`+`card-table`+badge desenine taşındı. Tarayıcıda üst+alt kategori oluşturularak nesting görsel olarak doğrulandı (`hasNestedGroup:true`), sonra temizlendi. Bu iki modülün RBAC/route/controller/tests'e hiç bağımlı olmadığı (Category/Collection için persisted xUnit içinde literal HTML/route testi yok) grep ile teyit edildi. 326/326 test değişmeden geçti.

**Takip #4 — "Çok çirkin" geri bildirimi sonrası Category/Create için detaylı UI/UX yeniden tasarımı (20.07.2026).** Kullanıcı Tabler entegrasyonunun görselini "hiç beğenmedim, ui/ux kurallarına göre çirkin" diye eleştirdi; `AskUserQuestion` ile hangi noktanın sorun olduğu soruldu, kullanıcı serbest metinle çok ayrıntılı bir tasarım şartnamesi yazdı (topbar arama+kullanıcı dropdown'ı, sayfa genişliği, kart başlıklarına numara+ikon, alan sırası/placeholder/ikon, RTL, invalid-state, alt İptal/Kaydet çubuğu, sidebar logo/aktif-renk) ve kapsamı "önce sadece Kategori Ekle'de deneyelim" olarak sınırladı.
- **Paylaşılan bileşenler (tüm sayfaları etkiler, geriye dönük varsayılan davranış korunarak):** `_Layout.cshtml`'e `ViewData["ContainerClass"]` opt-in (varsayılan hâlâ `container-xl`, yalnızca Category/Create `container-fluid` seçti) eklendi. `_Topbar.cshtml` yeniden yazıldı — dekoratif arama kutusu (`#navbar-search-input`, backend'i yok, `site.js`'te Ctrl+K ile fokus kısayolu eklendi, formsuz olduğu için Enter hiçbir şey tetiklemiyor) + avatar/e-posta/rol'ü tek link'te birleştirip Logout'u Bootstrap dropdown menüsüne taşıyan bir kullanıcı menüsü eklendi (`ViewData["HasPageHeader"]` true olan sayfalarda topbar'ın küçük başlığı gizleniyor, aksi hâlde eski davranış birebir korunuyor). `_Sidebar.cshtml`'e "Ana Menü" grup etiketi + "NG" logo rozeti eklendi (route/rol mantığı hiç değişmedi). `site.css`'e tutarlı kart radius/shadow (`--ng-radius`/`--ng-shadow`), aktif-sekme bej/kahverengi arka plan+alt-çizgi, ve **`input-validation-error` class'ına kırmızı border** (ASP.NET Core'un `asp-for` tag helper'ının ModelState hatası olan alana otomatik eklediği class — jQuery/istemci doğrulaması gerekmeden, salt CSS ile Tabler invalid-state görünümü) eklendi.
- **Kritik bulgu — "dosya yükleme ikonu ekle" talebiyle çelişen kendi koşulu:** Talimat hem "Görsel Yolu'nun yanına yükleme ikonu ekle" hem "backend desteklemiyorsa işlevsiz/dekoratif bırakma, eklemesi ya da güvenli gizle" diyordu — `Category`'de gerçek dosya yükleme yok (`ImagePath` düz metin), bu yüzden buton **hiç eklenmedi** (ikinci koşul birinciyi geçersiz kılıyor, kendi kendisiyle çelişmiyor).
- **Category/Create.cshtml:** Kendi `page-header` bloğu eklendi (ikonlu avatar + büyük "Yeni Kategori" başlığı + açıklama + "Listeye Dön"), `ContainerClass`/`HasPageHeader` bayrakları set edildi.
- **`_CategoryForm.cshtml` (Create VE Edit ile paylaşılan partial — ikisi de otomatik güncellendi):** "1. Temel Bilgiler"/"2. Çeviriler" ikonlu kart başlıkları, her çeviri alanına Tabler `input-icon` (typography/link/list-details/tag/file-description) + istenen placeholder'lar, AR sekmesinde `dir="rtl"`, alt işlem çubuğu `d-grid d-sm-flex justify-content-sm-between` ile "İptal" (Index'e link) + "Kaydet" (mobilde tam genişlik/alt alta).
- **Doğrulama:** Clean build 0/0, **326/326 test değişmeden geçti**. Tarayıcıda: sekme/dropdown/arama-kısayolu/aktif-sekme rengi/kart radius'u JS ile computed-style okunarak doğrulandı; gerçek `DisplayOrder` Range hatası tetiklenip `.input-validation-error` class'ının ve kırmızı border'ın (`rgb(161,63,55)`) doğru göründüğü kanıtlandı; kategori oluştur→düzenle→sil uçtan uca akışı (yeni tasarımla) sorunsuz çalıştı; Dashboard/Product/Collection gibi paylaşılan-bileşen-etkili diğer sayfalarda regresyon aranmadı, konsol hatası çıkmadı.
- **Not (bu task'ın kapsamı dışı, ayrıca fark edildi):** `site.js`'teki mobil sidebar'ı otomatik kapatan kod `window.bootstrap.Collapse`'a bağımlı ama `window.bootstrap` bu Tabler derlemesinde global olarak **hiç expose edilmiyor** (`data-bs-toggle` attribute'ları yine de native/delegated event ile çalışıyor) — bu kod sessizce no-op oluyor olabilir, Tabler entegrasyonundan (Task 19) kalma, potansiyel önceden var olan küçük bir hata, düzeltilmedi.
- Kalan 7 modül (Document/ReferenceProject/Blog/News/Banner/Page/Dealer/FormSubmission/Role) ve Category/Collection Index/Edit'in kendi page-header'ları henüz bu yeni "zengin" düzene taşınmadı — kullanıcı onayı bekleniyor.

*(Önceki durum korunuyor: Task 18 — Dashboard — TAMAMLANDI 20.07.2026, bkz. aşağıdaki detay.)*

## Önceki Task Detayı — Task 18 — Dashboard (Analiz + Otomatik İmplementasyon) — TAMAMLANDI (20.07.2026).

Analiz aşaması kritik bir mimari/kapsam çelişkisi bulmadı — talimatın kendi otomatik-devam koşulu ("Dashboard, mevcut verileri salt okunur biçimde özetleyen; yeni entity, migration, audit sistemi veya frontend paketi gerektirmeyen bir MVC ekranıdır") sağlandığı için aynı oturumda implementasyona geçildi.

**Analiz bulguları:** Madde 17.2'nin 15 modüllük tablosunda Dashboard yalnızca isim olarak geçiyor (#1) — kart/grafik/tablo tanımı yok. Mevcut Dashboard (`HomeController.Index` → `Home/Index.cshtml`) tamamen boştu ("Hoş geldiniz" mesajından ibaret). Kod taraması (tahmin değil) 6 entity'nin gerçek alanlarını doğruladı: `Product.Status` enum (`IsActive` yok, "aktif" = `Status==Active`), `Category`/`Collection`/`Dealer`/`ApplicationUser`'da `IsActive` bool, `FormSubmission`'da `IsRead` VE `ProcessedAt` **bağımsız iki alan** (Task 15'in Details GET'i bilinçli olarak otomatik-okundu-işaretlemediği için ayrım anlamlı — "okunmamış" ve "işlenmemiş" tek bir "Pending" alanına indirgenmedi, bilgi kaybı yaratırdı). `ApplicationUser`'da `CreatedAt` YOK (`IdentityUser` temel sınıfı sağlamıyor) — "son eklenen kullanıcılar" listesi bu yüzden desteklenmiyor, eklenmedi.

**Uygulanan kapsam:** 8 özet kartı (Ürün Toplam/Aktif, Kategori, Koleksiyon, Bayi, Showroom, Kullanıcı Toplam/Aktif, Form Toplam, Bekleyen Formlar [Okunmamış+İşlenmemiş]) + Son 5 Ürün + Son 5 Form Başvurusu tablosu. Dört rol de aynı kartları görür (dokümanda/kodda farklılaşma kanıtı yok) — **ancak** "Son Form Başvuruları" bölümü yalnızca `FormSubmissionController.ViewRoles`'e (Admin+İçerik Editörü) sahip roller için render ediliyor (SeoEditor/ProductManager'ın forma hiç erişimi yok, PII/link-güvenliği ihlali olmasın diye) — bu "farklı dashboard icat etme" değil, mevcut RBAC sınırına saygı (`Product/Index.cshtml`'deki `canEdit` deseniyle birebir aynı yaklaşım).

**Mimari bulgu — `DashboardService` doğrudan `AppDbContext`'e bağımlı:** Mevcut 6 modül repository'sinin hiçbirinde `CountAsync`/predicate yok (hepsi `GetAllAsync()` tam liste döndürüyor). Bunlara yeni metod eklemek yerine `DashboardService` (Infrastructure) doğrudan `AppDbContext` kullanıyor — ADR-016'nın (Application Infrastructure'a referans veremiyor) Identity-özel olmayan, genel bir uygulanışı; ADR-016'ya bu genişlemeyi kaydeden not eklendi. Tüm sorgular `AsNoTracking()` + DB-seviyesi `CountAsync()`/`OrderByDescending().Take(5)` — hiçbir tam tablo belleğe çekilmedi, N+1 yok (Recent Products/Forms projeksiyonları navigation property kullanmıyor).

**Önemli düzeltme (doküman-vs-gerçeklik):** Talimat "mevcut Bootstrap" kullanılmasını istiyordu ama projede **hiç CSS framework kurulu değil** (`wwwroot` yok) — 17 önceki task boyunca çıplak semantik HTML kullanılmış. Dashboard da aynı konvansiyonu izledi, yeni framework eklenmedi.

**Doğrulama:** Clean build 0/0; solution toplamı **326/326 test** (178 unit [163+15 yeni] + 148 integration [134+14 yeni]). Coverage: `DashboardService`/`DashboardDto`/`DashboardRecentProductDto`/`DashboardRecentFormDto`/`DashboardViewModel` %100. Yeni migration/paket yok.

**Sıradaki iş:** SEO veri sözleşmesi (#4), alan-seviyeli RBAC (#23), Excel Import, Docker Desktop çalışır hale geldiğinde canlı doğrulama, veya kalan modüllerin unit testlerinin genişletilmesi — henüz başlanmadı, onay bekliyor.

*(Önceki durum korunuyor: Task 17 — Role Management — TAMAMLANDI 20.07.2026, bkz. aşağıdaki detay.)*

## Önceki Task Detayı — Task 17 — Role Management (Analiz + Otomatik İmplementasyon) — TAMAMLANDI (20.07.2026).

Analiz aşaması, doküman + kod taramasıyla kritik bir mimari/kapsam çelişkisi bulunmadığını (Seçenek A — sabit 4 rol için salt-okunur ekran — tek tutarlı seçenek) tespit etti; talimatın kendi otomatik-devam koşulu sağlandığı için aynı oturumda implementasyona geçildi, kullanıcıya soru sorulmadı.

**Analiz bulguları:** Madde 17.2'nin 15 modüllük tablosunda "Kullanıcı/Rol Yönetimi" **tek satır** (#14) — User Management'tan ayrı bir "Role Management" modülü doküman düzeyinde hiç yok. Madde 7.2 dört rolü (Admin/İçerik Editörü/SEO Editörü/Ürün Yöneticisi) kapalı bir liste olarak tanımlıyor, dinamik rol oluşturma/silmeye dair sıfır kanıt var. Mevcut RBAC tamamen derleme-zamanı `[Authorize(Roles=...)]` sabitlerine dayandığından (kod taranarak doğrulandı) yeni bir rol oluşturulsa bile hiçbir controller'da otomatik erişim kazanmaz — bu, dinamik rol CRUD'ı (Seçenek C) hem dokümansız hem de işlevsiz kılıyor. Seçenek B'nin öncülü olan "teknik ad/görünen ad" ayrımı da bu sistemde hiç yok (`ApplicationRoles` sabitleri zaten Türkçe görünen ad, `IdentityRole.Name`/`[Authorize(Roles=...)]` ile birebir aynı string) — bu da B'yi dokümansız/gereksiz kılıyor.

**Uygulanan kapsam (Seçenek A):** `RoleController` yalnızca `Index`/`Details` GET action'ları — hiçbir state-changing action yok, AntiForgery/PRG gerekmiyor. Yetki matrisi, gerçek her controller'ın `ViewRoles`/`EditRoles` sabitlerinden **elle** taranarak çıkarılan statik bir veri yapısı (`RoleManagementService.PermissionMatrix`) — dinamik/reflection-tabanlı otomatik keşif kasıtlı olarak yapılmadı. Henüz controller'ı olmayan 3 modül (Dil/SEO/Excel Import) "Henüz Uygulanmadı" olarak işaretlendi, tahmini bir erişim seviyesi verilmedi.

**Kritik mimari bulgu — `IUserManagementService` deseni ikinci kez kullanıldı:** `IRoleManagementService`/`RoleManagementService`, ADR-016'daki arayüz-Application/implementasyon-Infrastructure desenini tekrar kullandı (aynı gerekçe: `RoleManager<IdentityRole>`/`UserManager<ApplicationUser>`'a bağımlılık). ADR-016'nın "projedeki tek örnek" ifadesi bu genişlemeyle güncel değildi — ADR-016'ya güncelleme notu eklendi (Task 17, 20.07.2026), artık projede Identity'ye bağımlı **iki** interface'li servis var.

**Doğrulama:** Clean build 0/0; solution toplamı **297/297 test** (163 unit [148+15 yeni] + 134 integration [121+13 yeni]). Coverage: global %16.4 line (Task 16B'nin %15.7'sinden yükseldi); `RoleManagementService` %100, `RoleController` %100, tüm yeni DTO'lar/View'lar %100.

Backlog madde #2 ("Kullanıcı/Rol Yönetimi") **tamamen kapatıldı** — Role Management artık salt-okunur denetim ekranı olarak TAMAMLANDI, dinamik rol CRUD/permission sistemi bilinçli olarak kapsam dışı kaldı (ayrı bir backlog maddesi değil — dokümanda dayanağı olmadığı için hiç açılmadı).

*(Önceki durum korunuyor: Task 16 / 16B — Kullanıcı Yönetimi — TAMAMLANDI 20.07.2026, bkz. aşağıdaki detay.)*

## Önceki Task Detayı — Task 16 / 16B — Kullanıcı Yönetimi (Analiz + İmplementasyon) — TAMAMLANDI (20.07.2026).

İki alt-adımda yürütüldü: **Task 16** (analiz-only, kod değişikliği yasak) 30 başlıklı bir rapor üretti ve 3 kritik belirsizliği (aktif/pasif mekanizması, parola belirleme, silme davranışı) `AskUserQuestion` ile netleştirdi; **Task 16B** o kararlarla uçtan uca implemente etti.

**Kesinleşen kararlar (kullanıcı onayı, Task 16 sonu):**
1. **Aktif/Pasif:** Yeni `ApplicationUser.IsActive` (bool, varsayılan true) — Identity'nin Lockout mekanizmasından (başarısız-giriş korumasına özel) bilinçli olarak ayrı tutuldu.
2. **Parola:** Admin formda doğrudan belirler — `CreateAsync(user, password)` (yeni kullanıcı) / `GeneratePasswordResetTokenAsync`+`ResetPasswordAsync` (sıfırlama, e-posta göndermeden sunucu içinde üretilip aynı istekte tüketilir). Proje genelinde SMTP/e-posta altyapısı yok (Task 15'te doğrulanmıştı).
3. **Silme:** Hard-delete izinli — kendi-hesap ve son-aktif-Admin guardrail'leriyle birlikte.

**Task 16B implementasyon-öncesi revizyon (kullanıcı talimatı, 20.07.2026 — implementasyon başlamadan hemen önce):**
4. **Tek rol (çoklu rol desteği kaldırıldı):** Referans dokümandaki roller (Admin/İçerik Editörü/SEO Editörü/Ürün Yöneticisi) birbirini dışlayan iş rolleridir; MVP kapsamında bir kullanıcı aynı anda yalnızca bir role sahip olabilir. `CreateUserRequest.Role`/`UpdateUserRequest.Role`/`UserDto.Role` tekil `string` (eski `Roles: IReadOnlyList<string>` yerine). Create/Edit formlarında checkbox listesi yerine tek-seçim dropdown. `AspNetUserRoles` tablosu ve Identity mimarisi değişmedi — kural yalnızca `UserManagementService` (uygulama) seviyesinde: `CreateAsync` tam olarak bir rolü `AddToRoleAsync` ile atar, `UpdateAsync` rol değiştiğinde eski rolü `RemoveFromRoleAsync` ile kaldırıp yenisini `AddToRoleAsync` ile ekler.
5. **Email/UserName değiştirilemez:** Email hem giriş bilgisi hem kullanıcı adı olduğu için oluşturulduktan sonra sabittir — `UpdateUserRequest`'te artık `Email` alanı yok, `UpdateAsync` `SetEmailAsync`/`SetUserNameAsync` çağırmıyor. Edit formunda Email salt-okunur metin olarak gösterilir (input olarak render edilmez); Create formunda normal zorunlu alan olarak kalır.

**Kritik mimari bulgu — `IUserManagementService` neden interface (Task 17'de ikinci kez kullanıldı, bkz. yukarısı):** `Application` projesi `Infrastructure`'a referans veremiyor (mevcut graph: Presentation→(Application,Infrastructure); Application→Domain; Infrastructure→(Application,Domain)) — ama implementasyon `UserManager<ApplicationUser>`/`RoleManager<IdentityRole>`'a (Infrastructure.Identity) bağımlı olmak zorunda. Çözüm: `IUserManagementService` + saf primitive DTO'lar (`Application/Users/`) Application'da, implementasyon (`UserManagementService`) Infrastructure'da (`Infrastructure/Identity/`) — projenin her Repository'sinin arayüz-Application/implementasyon-Infrastructure deseniyle birebir tutarlı.

**Login davranışı:** `AccountController.Login` — `user is not null && user.IsActive` kontrolü `PasswordSignInAsync`'ten ÖNCE yapılıyor (pasif kullanıcı için AccessFailedCount/lockout sayaçları anlamsız yere işlemesin + "kullanıcı yok" ile "kullanıcı pasif" aynı genel hata mesajıyla dışarı yansısın, bilgi sızıntısı yok).

**Guardrail tasarımı:** `IsLastActiveAdminAsync(user)` tek doğruluk kaynağı — Delete/Deactivate/rol-kaldırma hepsi bunu kullanıyor ("hedef şu an aktif VE Admin VE ondan başka aktif Admin yok" → true). Kendi-hesap kontrolü (`user.Id == currentUserId`) her üç tehlikeli işlemde (Deactivate/Delete/rol-kaldırma) ayrı ayrı uygulanıyor. Bu kontroller yalnızca UI'da değil **servis katmanında** — controller bypass edilse bile korunuyor.

**Migration:** `AddIsActiveToApplicationUser` — yalnızca `AspNetUsers.IsActive` kolonu (bit, NOT NULL, DEFAULT 1). Mevcut seed edilmiş admin/dev-test kullanıcıları migration sonrası sqlcmd ile `IsActive=1` doğrulandı.

**Doğrulama (revizyon sonrası):** Clean build 0/0; solution toplamı **269/269 test** (148 unit + 121 integration — tek-rol/immutable-email revizyonu test SAYISINI değiştirmedi, mevcut testler yerinde güncellendi: çoklu-rol testi → "tam olarak bir rol atanır" testi, email-değiştirme testi → "email değişmez" testi).

*(Önceki durum korunuyor: Task 15 — Form Yönetimi — TAMAMLANDI 20.07.2026, bkz. aşağıdaki detay.)*

## Önceki Task Detayı — Task 15 — Form Yönetimi — TAMAMLANDI (20.07.2026).

Task 14 gibi gerçekten yeni bir modül (backlog #16, "Form Yönetimi" — Madde 17.2'de "İletişim ve bilgi talep formları listesi, durum takibi, e-posta bildirimleri"). Madde 29 (Formlar) tek tek incelendi: 29.1 İletişim Formu, 29.2 Request Information/Bilgi Talep Formu, 29.3 Numune Talep Formu — somut alan listesiyle tanımlanan yalnızca bu 3 form türü var. Bu task ayrıca projedeki **ilk gerçek SQL-seviyesi pagination/filtreleme** implementasyonunu getirdi (ADR-015).

**Kritik bulgu 1 — Modül kapsamı, dinamik form builder DEĞİL:** Madde 36.1'in "Forms | Form gönderileri | FormFields" ifadesi ve Madde 17.2'nin "Form Yönetimi" satırı yalnızca **gelen form kayıtlarının yönetimini** (liste, durum takibi, bildirim) tarif ediyor — hiçbir yerde admin'in yeni form tanımlayabildiği, alan tasarlayabildiği bir builder ekranı yok. Bu yüzden `FormDefinition`/`FormField`/drag-and-drop builder/JSON blob **kurulmadı** — tek `FormSubmission` entity + `FormType` enum discriminator + tip-özel nullable sütunlar (Seçenek 1: tek tablo + ortak alanlar + opsiyonel alanlar).

**Kritik bulgu 2 — Durum: Status enum yerine IsRead/ReadAt/ProcessedAt:** Madde 17.2 yalnızca soyut "durum takibi" diyor, somut değer listesi (Blog/News/Product'taki gibi) vermiyor. İcat edilmiş bir enum yerine nullable zaman damgaları kullanıldı (`ProcessedAt` dolu = işleme alındı) — Banner'ın PublishStartDate/EndDate deseniyle tutarlı.

**Kritik bulgu 3 — Translation ve dosya eki hiç kullanılmadı:** Tüm alanlar kullanıcı tarafından girilen ham veri — Dealer'dan sonra Translation'ı hiç tüketmeyen ikinci modül. Madde 29.1/29.2/29.3'ün hiçbirinde dosya/CV eki yok — `IFileStorageService` kullanılmadı.

**Kritik bulgu 4 — Public form gönderim endpoint'i ve e-posta bildirimi bu fazda kurulmadı:** ADR-001/002/009 gereği public site kodu bu fazda hiç yazılmıyor. `FormSubmissionService.CreateSubmissionAsync` Application katmanında hazır ve test edilmiş (gelecekteki public site doğrudan çağırabilir) ama hiçbir `[AllowAnonymous]` controller'dan çağrılmıyor. E-posta bildirimi de bu var-olmayan public akışın sonucu olduğu için kurulmadı — SMTP/MailKit altyapısı eklenmedi.

**Kritik bulgu 5 (asıl yeni mimari desen, ADR-015) — Projedeki ilk gerçek SQL-seviyesi pagination:** Form kayıtları zamanla sürekli büyüyen bir veri seti — `IFormSubmissionRepository.GetPagedAsync(FormSubmissionQuery)` gerçek `IQueryable.Where/OrderByDescending/Skip/Take` + ayrı `CountAsync()` kullanıyor, hiçbir noktada tüm tablo belleğe çekilmiyor (önceki tüm modüllerin `GetAllAsync()`+in-memory-filtreleme deseninden bilinçli sapma). Bundan sonraki modüller için ilke ADR-015'te kayıtlı: sürekli büyüyen veri setlerinde (log/audit, form/lead) pagination deseni tercih edilmeli, işletme-yönetimli sınırlı veri setlerinde (kategori/ürün) mevcut in-memory desen yeterli.

**RBAC:** Madde 30 Form Yönetimi satırı: Admin=Tam, İçerik Editörü=Görüntüleme, SEO Editörü=—, Ürün Yöneticisi=—. `FormSubmissionController`'da ViewRoles=Admin+İçerikEditörü, EditRoles=yalnızca Admin. Details GET action'ında **bilinçli olarak** otomatik okundu-işaretleme yapılmadı (GET idempotent kalmalı + İçerik Editörü'nün "Görüntüleme" yetkisiyle dolaylı yazma tetiklememesi için) — okundu işaretleme ayrı, yalnızca Admin'e açık bir POST action.

**Domain:** `FormSubmission` (Id, FormType, FullName/Email/Phone zorunlu, Company opsiyonel, Message zorunlu, ConsentAccepted; Subject/ProductCode+ProductName/Address+RequestedProduct+Quantity tip-özel nullable; IsRead+ReadAt, ProcessedAt, AdminNote, CreatedAt). `Domain/Enums/FormType.cs` — Contact/RequestInformation/SampleRequest.

**Application:** `Application/Forms/` — `FormSubmissionService` (Translation/IFileStorageService kullanmıyor; FormType'a göre koşullu zorunlu alan doğrulaması; e-posta regex ile GeneratedRegex — AntiForgeryHelper'daki desenle tutarlı), `FormSubmissionQuery`/`PagedResult<T>` (pagination/filtre parametreleri).

**Infrastructure:** `FormSubmissionConfiguration` (2 index: `CreatedAt` — her Index sayfası yüklemesi bununla sıralanıyor; `FormType+CreatedAt` — tip filtresi gerçek sorgu deseni. IsRead/Email/Phone'a index eklenmedi — düşük kardinalite/LIKE-arama sınırlı fayda, gerekçeli "şimdi değil"), `FormSubmissionRepository` (gerçek `IQueryable` filtreleme+sayfalama).

**Presentation:** `FormSubmissionController` (Index [filtre+pagination]/Details/MarkAsRead/MarkAsUnread/MarkAsProcessed/MarkAsUnprocessed/UpdateAdminNote/Delete), liste ekranında Message/AdminNote gibi ayrıntılı kişisel veri **gösterilmiyor** (yalnızca Details'te) — Razor otomatik HTML-encode ile XSS'e karşı korunuyor, ayrı bir sanitize kütüphanesi eklenmedi.

**Migration:** `AddFormSubmissions` — yalnızca `FormSubmissions` tablosu + 2 index; başka hiçbir tabloya dokunmadı.

**Doğrulama:** Clean build 0 Warning/0 Error; solution toplamı **202/202 test geçti** (116 unit [92+24 yeni] + 86 integration [73+13 yeni]). Coverage: global %15.1 line (Task 14'ün %14.3'ünden yükseldi); `FormSubmissionService` %85.2, `FormSubmissionRepository` %92.6 (pagination/filtre kodu iyi test edilmiş). Docker'a dokunulmadı.

**ADR-015** eklendi — tek tablo+discriminator kararı + projedeki ilk gerçek pagination deseninin gelecekteki modüller için ilke olarak kaydı.

Backlog madde 16 ("Form Yönetimi") **tamamen tamamlandı** işaretlendi (yalnızca admin veri modeli/servis/UI — public gönderim endpoint'i ve e-posta bildirimi public site fazına bırakıldı, açık teknik borç olarak kaydedildi).

**Sıradaki iş:** SEO veri sözleşmesi (#4), alan-seviyeli RBAC (#23), Docker Desktop çalışır hale geldiğinde canlı doğrulama, kalan 5 modülün (Category/Collection/Document/ReferenceProject/Banner) unit testlerinin genişletilmesi, veya Dil/SEO/Kullanıcı-Rol/Excel-Import/Dashboard modüllerinden biri — henüz başlanmadı, onay bekliyor.

*(Önceki durum korunuyor: Task 14 — Bayi/Showroom Yönetimi — TAMAMLANDI 19.07.2026, bkz. aşağıdaki detay.)*

## Önceki Task Detayı — Task 14 — Bayi / Showroom Yönetimi — TAMAMLANDI (19.07.2026).

Bu task, Task 13'ün aksine **gerçekten yeni bir modül** — TASKS.md backlog madde 11 ("Bayi/Showroom") daha önce hiç başlanmamıştı. ADR-008 (Task 0.2) mimari kararı zaten "Tek `Dealer` entity + `Category` alanı" ilkesini onaylamıştı ama üç önemli nokta bilinçli olarak açık bırakılmıştı: kategorisiz (17) kayıtların ele alınışı, showroom-özel alanların (galeri/çalışma saatleri/randevu formu) bu fazda uygulanıp uygulanmayacağı, ve alan-bazlı Translation/görsel kararları. Bu task, referans dokümanın 25/25.1/25.2/26/30/36.1/Ek-2 maddelerini tek tek inceleyerek bu açık noktaları kesinleştirdi.

**Kritik bulgu 1 — 212/187/8/17 sayıları doğrulandı:** Madde 25 ("187 bayi (kategori 2), 8 showroom (kategori 3), 17 kategorisiz kayıt") ve Ek-2 ("Toplam Kayıt 212, Bayi (Kategori 2) 187, Showroom (Kategori 3) 8, Kategorisiz Kayıt 17, Benzersiz Şehir 63") birebir aynı sayıları veriyor — bu rakamlar dokümanın kendi içinde tutarlı, gerçek kaynak veriye dayanıyor (fabrikasyon değil). Bu task'ta **212 kayıt seed edilmedi** (talimatla açıkça yasaklandı) — yalnızca şema/enum kuruldu, gerçek veri aktarımı Excel Import'un (#17) konusu.

**Kritik bulgu 2 — Madde 25.1'in veri modeli tablosu, Product/Blog/Proje'nin aksine hiçbir alanı "(multi-lang)" işaretlemiyor:** `Name, City, District, Address, Phone, Fax, Email, Latitude, Longitude, Category, Region, RegionName, Status` — 13 alanın hepsi düz native alan. Bu, **projenin Translation altyapısını hiç tüketmeyen ilk CMS modülü** olduğu anlamına geliyor — `EntityType.Dealer` (Task 3.1B'den beri rezerve) bu task'ta da tüketilmedi, rezerve kalmaya devam ediyor. Aynı gerekçeyle SEO alanları (SeoUrl/MetaTitle/MetaDescription) da eklenmedi — Madde 27.2'nin "Bayi: /{dil}/satis-noktalari/{il}/{ilce}" URL deseni per-kayıt özel bir slug değil, City+District'ten programatik türetilen bir public-site deseni.

**Kritik bulgu 3 — Görsel/logo, galeri, açıklama, çalışma saatleri, randevu formu, sıralama: hiçbiri eklenmedi.** Madde 25.1'in gerçek veri modeli tablosunda bu alanların hiçbiri yok — yalnızca Madde 26'nın "showroom sayfaları... galeri görselleri, açıklama, çalışma saatleri... yer alacaktır" ifadesinde geçiyorlar, ama aynı paragraf modül ayrımını da "Karar Bekleniyor" bırakıyor ve bu, ADR-008'de zaten "otomatik zorunlu değil" olarak işaretlenmişti. Bu, dokümanın **public site anlatımı** (ADR-001/002/009 gereği bu fazın kapsamı dışı) — admin panelinin CRUD veri modelini bağlamıyor. `IFileStorageService` bu task'ta **hiç kullanılmadı** — projenin storage kurulduktan sonraki (Task 5.1'den beri) ilk görselsiz CMS modülü.

**Kritik bulgu 4 — Kategorisiz (17) kayıt kararı:** `Dealer.Category` **nullable** (`DealerCategory?`) yapıldı — yeni bir "Unclassified/Diğer" enum üyesi icat edilmedi, mevcut projenin nullable-FK deseniyle (BlogCategoryId?/NewsCategoryId?) tutarlı bir "henüz sınıflandırılmamış" temsili. Enum'un alttaki sayısal değerleri dokümanla birebir eşleştirildi: `Dealer = 2, Showroom = 3` (0/1 değil) — gelecekteki Excel Import'un legacy kod eşlemesiyle uyumlu olması için.

**Kritik bulgu 5 — RBAC, projedeki ilk salt-Admin modül:** Madde 30'un Bayi/Showroom satırı: "Tam | — | — | —" — Admin=Tam, İçerik Editörü=—, SEO Editörü=—, Ürün Yöneticisi=—. Diğer tüm modüllerde en az bir rol (genellikle SEO Editörü veya İçerik Editörü) salt-görüntüleme yetkisine sahipti; burada **hiçbir rol Admin dışında hiçbir erişime sahip değil**. `DealerController`'da tek bir `[Authorize(Roles = ApplicationRoles.Admin)]` sınıf seviyesi attribute'u yeterli oldu, ayrı ViewRoles/EditRoles ayrımına gerek kalmadı.

**Domain:** `Dealer` (Id, Name, City zorunlu; Category nullable enum; District/Address/Phone/Fax/Email/Region/RegionName opsiyonel string; Latitude/Longitude nullable `decimal(9,6)`; IsActive bool + Activate/Deactivate — Category/Collection/Banner'ın bool-IsActive deseni). `Domain/Enums/DealerCategory.cs` — `Dealer = 2, Showroom = 3`.

**Application:** `Application/Dealers/` — `DealerService` (Translation ve IFileStorageService **hiç kullanmıyor**, projedeki en yalın servis; Name/City zorunluluğu, Latitude -90..90/Longitude -180..180 aralık doğrulama, "ikisi birlikte veya hiçbiri" koordinat kuralı, ToggleActive). Mevcut `IUnitOfWork` değiştirilmeden tekrar kullanıldı; yeni genel-amaçlı abstraction eklenmedi.

**Infrastructure:** `DealerConfiguration` (Category `HasConversion<string>()`, Latitude/Longitude `decimal(9,6)` — float/double kullanılmadı), `DealerRepository`.

**Presentation:** `DealerController` (Index/Create/Edit/Delete/ToggleActive — Admin-only), Index'te Category+City filtreleme (uygulama-seviyesi/in-memory, SQL'e itilmiyor — projenin genel listeleme desenine tutarlı).

**Index/SEO review sonucu — yeni index gerekmedi:** `DealerRepository` hiçbir `.Where`/`.OrderBy` içermiyor (Index'teki Category/City filtresi DTO listesi üzerinde in-memory LINQ) — gerçek SQL sorgu kanıtı olmadığı için hiçbir yeni index eklenmedi, mevcut PK yeterli.

**Migration:** `AddDealers` — yalnızca `Dealers` tablosu; başka hiçbir tabloya dokunmadı.

**Doğrulama:** Clean build 0 Warning/0 Error; solution toplamı **165/165 test geçti** (92 unit [75+17 yeni] + 73 integration [56+17 yeni]). Coverage: global %14.3 line (Task 13'ün %14.1'inden hafif yükseldi); `DealerService` %93.4 line — projedeki en yüksek servis coverage'ı. Docker'a dokunulmadı (talimatla açıkça yasaklandı).

**Yeni ADR gerekmedi** — mevcut ADR-008 güncellendi (yeni bir karar değil, ADR-008'in bilinçli olarak açık bıraktığı 3 noktanın bu task'ta kesinleştirilmesi).

Backlog madde 11 ("Bayi/Showroom") **tamamen tamamlandı** işaretlendi.

**Sıradaki iş:** SEO veri sözleşmesi (#4), alan-seviyeli RBAC (#23), Docker Desktop çalışır hale geldiğinde canlı doğrulama, veya kalan 5 modülün (Category/Collection/Document/ReferenceProject/Banner) unit testlerinin genişletilmesi — henüz başlanmadı, onay bekliyor.

*(Önceki durum korunuyor: Task 13 — Haber Yönetimi Test Sertleştirmesi — TAMAMLANDI 19.07.2026, bkz. aşağıdaki detay.)*

## Önceki Task Detayı — Task 13 — Haber Yönetimi (News) Test Sertleştirmesi — TAMAMLANDI (19.07.2026).

Prompt "Task 13 — Haber Yönetimi'ni uçtan uca geliştir" diye geldi, ama önce PROJECT_MEMORY.md/TASKS.md/PROGRESS.md okunduğunda **Haber Yönetimi'nin zaten Task 9'da (19.07.2026) uçtan uca tamamlanmış** olduğu görüldü — Domain/Application/Infrastructure/Presentation/migration hepsi mevcut ve çalışır durumda, 31/31 senaryo o task'ta doğrulanmıştı. Bu, kullanıcının kendi "durman gereken durumlar" listesindeki "Mevcut Blog/News tabloları arasında veri kaybı riski taşıyan yeniden kullanım kararı gerekiyorsa" maddesiyle örtüştüğü için **AskUserQuestion ile durulup onay istendi** — kullanıcı "Mevcut News modülünü test standardına yükselt" seçeneğini onayladı: Domain/Application/Infrastructure/Presentation'a dokunmadan, Task 12'nin Blog/Page/PageContentBlock/Product için kurduğu kalıcı test standardını News/NewsCategory'ye de uygulamak.

**Yapılanlar (yalnızca test + doğrulama, iş katmanı değişmedi):**
- `tests/NGKutahyaSeramik.UnitTests/Factories/NewsFactory.cs` (+ `NewsCategoryFactory`) — `BlogFactory`/`BlogCategoryFactory` deseninin birebir tekrarı.
- `NewsServiceTests.cs` (16 test) — TR başlık zorunluluğu, translation alan kaydı/temizliği, kategori doğrulama (var olmayan kategori reddi, geçerli kategori kabulü, kategorisiz kabul), kategori silme → News hayatta kalıyor (SetNull), her 3 Status değeri için oluşturma, PublishDate nullable, kapak görseli yükleme/değiştirme/kaldırma (dosya silme çağrıları), silme → Translation+fiziksel dosya temizliği.
- `NewsCategoryServiceTests.cs` (8 test) — TR ad zorunluluğu, negatif sıralama reddi, case-insensitive duplicate reddi, kendi adını koruyarak güncelleme, toggle active, silme → Translation temizliği, var olmayan kategori silme reddi.
- Integration testlere News eklendi: `AnonymousAccessTests` (+7 case: `/News`, `/News/Create`, `/News/Edit/1`, `/NewsCategory`, `/NewsCategory/Create` GET + `/News/Delete/1`, `/NewsCategory/Delete/1` POST), `RbacTests` (+8 case: ViewRoles/EditRoles/ProductManager-reddi/SeoEditor-reddi, News+NewsCategory), `AntiForgeryAndPrgTests` (+3: geçerli token+PRG, token'sız 400 reddi, geçersiz ModelState→200), `RelationalConstraintTests` (+1: `DeletingNewsCategory_SetsNewsCategoryIdToNull_NewsSurvives`, integration-seviyesinde SQLite ile).
- **Index/SEO review sonucu:** `NewsRepository.GetAllAsync()`/`NewsCategoryRepository.GetAllAsync()` incelendi — hiçbir `.Where`/`.OrderBy` SQL-seviyesi sorgu yok (Controller'daki `OrderByDescending(PublishDate)` DTO listesi üzerinde bellek-içi sıralama, SQL değil). Bu, Task 12'nin index audit'inde zaten "Blog/News Status+PublishDate — kod tabanında karşılık gelen sorgu yok, eklenmedi" kararıyla tam tutarlı — **yeni migration/index gerekmedi**, mevcut karar yeniden doğrulandı. SEO URL tekilleştirmesi hâlâ sistem genelinde yok (diğer tüm modüllerle aynı, yeni bir teknik borç değil — mevcut olanın tekrar teyidi).
- **Coverage:** `NewsService` %80.5 line, `NewsCategoryService` %88.6 line (Task 12'deki Blog/Page seviyesiyle tutarlı). Global: %14.1 line/%24.2 branch/%40.7 method (Task 12'nin %11.5'inden yükseldi — 5. modül test kapsamına girdi).
- **Test sayıları:** 75 unit (51+24 yeni) + 56 integration (37+19 yeni) = **131/131 geçti**, 0 başarısız, 0 atlanmış.
- Migration **oluşturulmadı** (gerçek bir şema eksikliği bulunmadı). Docker'a **dokunulmadı** (talimat gereği, canlı doğrulama final stabilization task'ına bırakıldı). Mevcut News/NewsCategory kodu, migration'ı, controller'ları, view'ları **hiç değiştirilmedi**.

Backlog'da yeni madde açılmadı — TASKS.md madde 21'in ("Test senaryolarının panel/API tarafına uygulanması") kapsamı News eklenerek genişledi (Blog/Page/PageContentBlock/Product/News artık test kapsamında; Category/Collection/Document/ReferenceProject/Banner hâlâ bekliyor).

**Sıradaki iş:** Docker Desktop çalışır hale geldiğinde canlı `docker compose up` doğrulaması, kalan 5 modülün (Category/Collection/Document/ReferenceProject/Banner) unit testlerinin genişletilmesi, SEO veri sözleşmesi (#4), alan-seviyeli RBAC (#23) veya Bayi/Showroom Yönetimi (#11) — henüz başlanmadı, onay bekliyor.

*(Önceki durum korunuyor: Task 12 — Testing Foundation — TAMAMLANDI 19.07.2026, bkz. aşağıdaki detay.)*

## Önceki Task Detayı — Task 12 — Testing Foundation, Test Altyapısı, Seeder Sertleştirme, Docker ve Migration/Index Audit — TAMAMLANDI (19.07.2026).

Bu task, önceki 11 task'ın aksine **yeni bir CMS modülü değil** — projenin ilk kalıcı otomatik test altyapısı, seeder güvenilirliği, konteynerleştirme ve şema denetimi task'ı. Talimat 21 madde + "durulması gereken durumlar" + "kısıtlar" listesiyle çok kapsamlıydı; hiçbir kısıtlama ihlal edilmedi (MediatR/CQRS eklenmedi, Repository/Service pattern korundu, gerçek dış API çağrısı yapılmadı, production DB'ye dokunulmadı, eski migration'lar geriye dönük değiştirilmedi, AntiForgery hiçbir testte devre dışı bırakılmadı).

**Kritik bulgu 1 — Page↔PageContentBlock ilişkisi (doğrulama, düzeltme değil):** Task 11'in kapanış raporundaki "1:1-sahipli" ifadesi belirsiz/yanıltıcı bulundu, kod incelendi: `PageContentBlockConfiguration.cs`'te `builder.HasOne(b => b.Page).WithMany().HasForeignKey(b => b.PageId)` — gerçek ilişki **bire-çok (Page 1 --- N PageContentBlock)**, entity/migration/kod hiçbir zaman yanlış değildi. Yalnızca **dokümantasyon dili** yanlıştı ("1:1-sahipli" çok kolonlu bir sahiplik ilişkisini çağrıştırıyor, cardinality'yi değil). Kod/migration/schema değişikliği **yapılmadı** — yalnızca PROGRESS.md'deki 3 yanlış ifade "bire-çok"/"tek-sahipli" olarak düzeltildi ve `PageContentBlockConfiguration.cs`'teki yorum satırı "Page 1 --- N PageContentBlock (bire-çok)" olarak netleştirildi. `RelationalConstraintTests.cs`'te bir integration test bu davranışı (bir Page'in birden fazla PageContentBlock'u olabildiğini, Page silindiğinde bloklarının Cascade ile silindiğini) doğrular.

**Kritik bulgu 2 — Unit/Integration ayrımı ve "sociable unit test" kararı:** Talimat, servis testlerinin `UseInMemoryDatabase` **kullanmamasını** ve gerçek ilişkisel davranışın (FK/unique/cascade) sınanmasını zorunlu kılıyordu. Ama `BlogService`/`PageService`/`ProductService` gibi servislerin "Create" akışı, entity `Id`'sinin (private-set, yalnızca EF `SaveChanges` ile atanan) DB round-trip'ine bağımlı — saf Moq ile mantıklı test edilemiyordu. Çözüm: **SQLite in-memory** ile gerçek repository+UnitOfWork'ü kullanan, yalnızca gerçek dış sınırları (dosya I/O, Translation persistence — ikisi de zaten interface arkasında) sahteleyen "sociable unit test" deseni benimsendi ve kod içinde/TESTING.md'de gerekçelendirilerek belgelendi.

**Kritik bulgu 3 — SQLite/SQL Server uyumsuzlukları:** Üretim modeli iki noktada SQLite'ta doğrudan çalışmıyordu: (a) `Translation.Value`'nun `nvarchar(max)` raw column type'ı SQLite DDL'inde sözdizimi hatası veriyordu, (b) üretim veritabanının gerçek collation'ı (`SQL_Latin1_General_CP1_CI_AS`, sqlcmd ile ampirik doğrulandı) case-insensitive iken SQLite varsayılanı case-sensitive. Üretim configuration/migration dosyalarına **dokunulmadan**, yalnızca test projesinde devreye giren özel bir `IModelCustomizer` (`SqliteCompatibleModelCustomizer`) ile her iki fark da adapte edildi (nvarchar(max) tipini temizleme + tüm string kolonlara `NOCASE` collation uygulama).

**Kritik bulgu 4 — Seeder sertleştirme:** Mevcut `IdentitySeeder`/`LanguageSeeder` zaten idempotent'ti (Task 1.2B/3.1B'de kurulmuştu) ama hiç otomatik testi yoktu ve seed'in ne zaman/nasıl çalışacağı `Program.cs`'e sabit kodlanmıştı (production'da migration/seed'in nasıl tetikleneceği ADR-004'te bilinçli olarak açık bırakılmıştı). Bu task'ta: (a) `IdentitySeederTests`/`LanguageSeederTests` eklendi (ilk/ikinci çalıştırma, duplicate yok, parola/rol overwrite yok, eksik config'te güvenli atlama), (b) `DatabaseInitialization:ApplyMigrationsOnStartup`/`SeedOnStartup` iki yeni config bayrağı eklendi (varsayılan `false`/`true`) — bu, ADR-004'ün açık bıraktığı "production'da migration nasıl tetiklenecek" sorusunu **çözüyor**, ama mevcut ADR-004'ü geçersiz kılmıyor, yalnızca tamamlıyor (yeni ayrı ADR gerekmedi — ADR-004 zaten "CLI ile veya `Database.MigrateAsync()` ile, karar sonraya bırakıldı" diyordu, bu task o kararı config-driven biçimde kesinleştirdi).

**Kritik bulgu 5 — Docker altyapısı hiç yoktu:** Projede daha önce hiçbir Dockerfile/docker-compose.yml yoktu. Bu task'ta sıfırdan oluşturuldu: multi-stage `Dockerfile`, `docker-compose.yml` (uygulama + SQL Server + healthcheck + `depends_on: condition: service_healthy` + kalıcı volume), `.env.example` (gerçek `.env` **commit edilmedi**, `.gitignore`'a eklendi). **Docker Desktop bu makinede bu oturum boyunca çalışır duruma getirilemedi** (`npipe` bağlantı hatası, engine başlatma denemeleri başarısız/yarım kaldı) — bu, dürüstçe bir sınırlama olarak raporlanıyor (bkz. kapanış raporu §18). Yerine, Docker'ın izleyeceği **aynı kod yolu** (config-driven migrate+seed) gerçek bir SQL Server'a karşı, boş/sıfırdan bir veritabanıyla iki kez çalıştırılarak (fresh migration + idempotent ikinci çalıştırma, checksum karşılaştırmalı) doğrulandı — geçici veritabanları (`NGKutahyaSeramikAdminPanel_FreshTest`, `_DockerStyleTest`, `_FinalCheck`) test sonrası temizlendi, gerçek geliştirme veritabanına hiç dokunulmadı.

**Kritik bulgu 6 — Migration/Index audit:** Tüm 12 migration dosyası tek tek okunarak gözden geçirildi — hiçbiri geriye dönük değiştirilmedi. Tüm `IEntityTypeConfiguration` dosyaları + repository sorgu desenleri (`.Where`/`.OrderBy` kullanımları) taranarak gerçek sorgu kanıtı olmadan index eklenmedi ("her koluna index" yaklaşımı reddedildi). İki gerçek eksik bulundu: `PageContentBlockRepository.GetByPageIdAsync` ve `ProductImageRepository`'nin eşdeğeri, ikisi de `WHERE X=@p ORDER BY DisplayOrder` çalıştırıyordu ama yalnızca tekil/filtered index'leri vardı — composite `(FK, DisplayOrder)` index'i eksikti. Tek, açık isimli corrective migration (`AddPerformanceAndConstraintIndexes`) ile düzeltildi; spekülatif indexler (Product.Status/Brand/CreatedAt, Blog/News Status+PublishDate) **eklenmedi** çünkü bu sorgu desenleri kod tabanında şu an mevcut değil — bilinçli "şimdi değil" kararı olarak kaydedildi.

**Test projeleri:** `tests/NGKutahyaSeramik.UnitTests` (51 test) + `tests/NGKutahyaSeramik.IntegrationTests` (37 test) — xUnit + FluentAssertions (6.12.1, lisans nedeniyle pinlendi) + Moq + `Microsoft.AspNetCore.Mvc.Testing` + `Microsoft.EntityFrameworkCore.Sqlite`. Toplam 88/88 test geçiyor (bkz. kapanış raporu). Ortak altyapı: `SqliteTestDatabase`, `SqliteCompatibleModelCustomizer`, `ServiceTestContext`, `IdentityTestHost`, `FakeUnitOfWork`, 9 model factory (`CategoryFactory`...`ImageUploadFactory`), `FakeTranslationService`/`FakeFileStorageService`, `CustomWebApplicationFactory`, `TestAuthHandler` (`X-Test-Role` header ile RBAC simülasyonu, AntiForgery'ye hiç dokunmuyor), `AntiForgeryHelper` (gerçek GET+token+POST akışı).

**Coverage:** Global 11.5% line / 18% branch / 32.9% method (273 sınıf, 239 dosya, projenin **10+ modülünün** bu task'ta hedeflenmediği için düşük — bilinçli kapsam sınırlaması). Hedeflenen sınıflarda güçlü kapsama: `BlogService` 76.3%, `PageService` 93.3%, `PageContentBlockService` 75.5%, `ProductService` 74.9%, `IdentitySeeder` 80.8%, `LanguageSeeder` 100%. Tüm `IEntityTypeConfiguration` sınıfları 100% (her `SqliteTestDatabase.Create()` çağrısında `OnModelCreating` çalıştığı için).

**Dokümantasyon:** `TESTING.md` sıfırdan oluşturuldu (test türleri, factory/mock kullanımı, coverage komutu, AntiForgery/RBAC test yaklaşımı, migration/index audit standardı, dış API mock standardı, gelecek modüller için Definition of Done checklist'i).

**Yeni ADR gerekmedi** — `DatabaseInitialization:ApplyMigrationsOnStartup`/`SeedOnStartup` ADR-004'ün önceden açık bıraktığı sorunun config-driven çözümü, ADR-004'ü değiştirmiyor/geçersiz kılmıyor.

Backlog'a yeni madde eklenmedi (bu task backlog modülü değil, altyapı task'ıydı). Docker Desktop'ın bu makinede çalışır hale getirilememesi **açık teknik borç/ortam kısıtı** olarak kaydedildi — kod/config tarafında yapılacak bir şey kalmadı, yalnızca gerçek bir Docker daemonu ile `docker compose up` çalıştırmak isteyen bir sonraki oturumun canlı doğrulaması gerekiyor.

**Sıradaki iş:** SEO veri sözleşmesi (#4), alan-seviyeli RBAC (#23), Bayi/Showroom Yönetimi (#11) veya Docker Desktop çalışır hale geldiğinde canlı `docker compose up` doğrulaması — henüz başlanmadı, onay bekliyor.

*(Önceki durum korunuyor: Task 11 — Sayfa Yönetimi — TAMAMLANDI 19.07.2026, bkz. aşağıdaki detay.)*

## Önceki Task Detayı — Task 11 — Sayfa Yönetimi (Backlog #12) — TAMAMLANDI (19.07.2026).

**Kritik bulgu 1 — Page'in projede ilk kez rastlanan "sessizliği":** Madde 16.2/17.2/30 Page için IsActive/Status/PublishDate/ParentId/DisplayOrder'ın **hiçbirinden** bahsetmiyor — Category/Collection/ReferenceProject/Banner'ın hepsinde "aktif/pasif" açıkça vardı, Blog/News'te Status enum açıkça vardı. Literal sadakatle hiçbiri eklenmedi. Kaldırma yalnızca hard-delete (Madde 30 zaten İçerik Editörü'ne "CRUD" — silme dahil — veriyor). Aynı gerekçeyle `ParentPage`/hiyerarşi ve `Menu` ilişkisi de eklenmedi — doküman ikisini de anmıyor, Madde 17.2'nin 15 modül listesinde ayrı bir Menü Yönetimi modülü yok.

**Kritik bulgu 2 — İçerik blok modeli (Madde 16.2, prose):** Tam metin: *"İçerik blokları esnek yapıda olacak: metin + görsel, tam genişlik görsel, video embed, akordeon, tab yapısı."* 5 blok tipi (`PageBlockType`: TextImage/FullWidthImage/VideoEmbed/Accordion/Tab) tek, düz bir `PageContentBlock` entity'sinde birleştirildi. **Accordion/Tab'ın çoklu panel/sekme grup yapısı dokümanda tanımlanmadığı için ayrı bir alt tablo veya grup kimliği (GroupKey/GroupId) icat edilmedi — her blok bağımsız bir içerik birimidir.** Ardışık aynı-tipli blokların birlikte bir akordeon/sekme grubu oluşturması yalnızca bir MVP/UI yorumudur, zorunlu bir domain kuralına dönüştürülmedi. Gelişmiş grup yönetimi gelecek faz kapsamındadır.

**Kritik bulgu 3 — Görsel modeli:** Madde 16.2 görselleri **blok** kavramının içinde tanımlıyor ("metin + görsel", "tam genişlik görsel") — `Page`'in kendi tekil/galeri görseli yok, görsel `PageContentBlock.ImagePath`'e ait (tek görsel/blok, galeri değil). Blok tipi bazlı zorunluluk kuralı: TextImage → TR içerik metni zorunlu, görsel opsiyonel; FullWidthImage → görsel zorunlu; VideoEmbed → `VideoEmbedUrl` zorunlu, görsel yüklenemez (reddedilir); Accordion/Tab → TR başlık zorunlu (panel başlığı/sekme etiketi olarak kullanılabilirlik gerekçesiyle).

**Kritik bulgu 4 — Blok tipi değişiminde otomatik çapraz-alan temizliği:** Blok tipi görsel-kullanmayan bir tipe değiştirildiğinde (ör. TextImage→VideoEmbed) mevcut görsel **otomatik** silinir (kullanıcı ayrıca işaretlemese bile) — hem DB hem fiziksel dosya. Blok tipi VideoEmbed'den başka bir tipe değiştirildiğinde `VideoEmbedUrl` **otomatik** temizlenir. Bu, projenin ilk "tip değişiminde çapraz-alan temizliği" kuralı — doğrulama testlerinde (12-13 numaralı senaryolar) bizzat test edildi.

**Kritik bulgu 5 — SEO Editörü yetkisi (alan-seviyeli RBAC — açık teknik borç):** Madde 30 Sayfa Yönetimi için SEO Editörü'ne "Meta Alanları" düzenleme yetkisi veriyor (yalnızca SeoUrl/MetaTitle/MetaDescription). Karar öncesi Blog/News/Product/Category/Collection/ReferenceProject controller'ları tek tek incelendi — **hiçbirinde alan-seviyeli RBAC altyapısı yok**, hepsi action-seviyeli (`ViewRoles`/`EditRoles`), SEO Editörü her zaman yalnızca `ViewRoles`'ta, hiçbir zaman `EditRoles`'ta. Bu task'ta da aynı proje konvansiyonu korundu: SEO Editörü sayfaları görüntüleyebilir ama SeoUrl/MetaTitle/MetaDescription dahil hiçbir alanı düzenleyemez, silemez ve tam CRUD/silme yetkisi de verilmedi. **Açık teknik borç olarak kaydedildi** ve TASKS.md'ye yeni backlog maddesi (#23 "Alan-seviyeli RBAC altyapısı") olarak eklendi — "Meta Alanları" gereksiniminin tam karşılanması bu ayrı task'a bağlı.

**Domain:** `Page` (`Id`, `CreatedAt`, `UpdatedAt` — başka native alan yok, Title/SeoUrl/MetaTitle/MetaDescription tamamen Translation'da), `PageContentBlock` (`Id`, `PageId`/`Page` FK Cascade, `BlockType` enum, `DisplayOrder`, `ImagePath?`, `VideoEmbedUrl?`, `CreatedAt`, `UpdatedAt`). `Domain/Enums/PageBlockType.cs` — 5 üye. `EntityType.Page` (Task 3.1B'nin orijinal 9-üyeli enum'unda zaten vardı, ilk kez bu task'ta tüketildi — Banner'ın `EntityType.Banner`'ı gibi) + yeni `EntityType.PageContentBlock` eklendi (enum + `EntityTypeMapping`, migration gerektirmedi — BlogCategory/NewsCategory'nin öngörülen genişletme mekanizmasının üçüncü uygulaması). Bloğun kendi Translation namespace'ine ihtiyacı vardı: `PageContentBlock.Id` ile `Page.Id` bağımsız sequence'lar olduğu için aynı `EntityType.Page` paylaşılsaydı bloklar ile sayfaların Translation kayıtları aynı (EntityType,EntityId) anahtarında çakışabilirdi.

**Application:** `Application/Pages/` — `PageService` (Category'nin Translation-CRUD deseni, ama native alan yok, yalnızca `CreatedAt`/`UpdatedAt` otomatik), `PageContentBlockService` (ProductImage/ReferenceProjectImage'ın görsel-doğrulama+telafi deseni + Banner'ın tekil-alan-çeviri deseni + blok-tipi-bazlı validasyon + tip-değişimi-temizliği — son ikisi hiçbir önceki modülde olmayan yeni bir kural seti, ama sıfır yeni genel-amaçlı abstraction). Sıralama ReferenceProjectImage'daki MoveUp/MoveDown swap deseniyle (manuel sayı girişi yok). Mevcut `ITranslationService`/`IUnitOfWork`/`IFileStorageService` **hiç değiştirilmeden** tekrar kullanıldı.

**Infrastructure:** `PageConfiguration`, `PageContentBlockConfiguration` (BlockType `HasConversion<string>()`, PageId FK Cascade), `PageRepository`, `PageContentBlockRepository`.

**Presentation:** `PageController` (Index/Details/Create/Edit/Delete — kullanıcı talimatıyla **Details** ayrı bir ekran olarak eklendi, önceki modüllerin aksine), `PageContentBlockController` (Create/Edit/MoveUp/MoveDown/Delete — Page'in Edit ekranına gömülü, ReferenceProjectImageController'ın dosya-yükleme deseni + Banner'ın çeviri-formu deseni birleşti). RBAC: Madde 30 satırı literal — Admin=Tam, İçerik Editörü=CRUD (silme dahil), SEO Editörü=Meta Alanları → proje konvansiyonuyla salt-görüntüleme'ye indirgendi, Ürün Yöneticisi=—. `PageContentBlockController` tamamen `EditRoles`'a kapalı.

**Migration:** `AddPages` — yalnızca `Pages`/`PageContentBlocks`; `Translations` şemasına dokunulmadı, başka hiçbir tabloya dokunulmadı.

**Doğrulama (45/45 iş kuralı/güvenlik senaryosu ✅):** Minimal TR-only create, TR başlık eksik reddi, çoklu dil+SEO create, upsert+opsiyonel-alan-silme (MetaDescription boşaltıldı), **5 blok tipinin her biri için oluşturma**, TextImage içerik-eksik reddi, FullWidthImage görsel-eksik reddi, VideoEmbed link-eksik reddi, **VideoEmbed+görsel kombinasyonu reddi**, Accordion/Tab başlık-eksik reddi, DisplayOrder sıralı atama, görsel değiştirme (eski dosya silinir), **blok tipi değişiminde eski görselin otomatik silinmesi**, **blok tipi değişiminde VideoEmbedUrl'in otomatik temizlenmesi**, görsel kaldırma, MoveUp sıralama, blok silme, **sayfa silme → tüm blokların+Translation'ların+fiziksel dosyaların temizlenmesi** — tamamı geçici doğrulama koduyla (`PageService`/`PageContentBlockService` doğrudan çağrılarak) test edildi, 45/45 geçti. sqlcmd ile tam temizlik doğrulandı (Pages/PageContentBlocks/Translations[PAGE,PAGE_CONTENT_BLOCK] tümü 0 satır, Identity/Languages etkilenmedi — 2 kullanıcı/4 rol/7 dil aynı). Uygulama gerçekten başlatılıp 6 endpoint'te anonim erişimin Login'e yönlendirdiği ve POST endpoint'lerin de aynı şekilde yetkisiz reddedildiği curl ile doğrulandı; unhandled exception yok.

Geçici doğrulama kodu (`PageVerification.cs` + `Program.cs`'teki `--verify-pages` bloğu) tamamen kaldırıldı; test sırasında oluşan `wwwroot/uploads/pages` dosyaları servis katmanının kendi telafi/temizlik mantığıyla otomatik silindi (manuel ek temizlik gerekmedi).

**Yeni ADR gerekmedi** — ADR-006/013/014 deseni yedinci kez tekrar kullanıldı; `EntityType.PageContentBlock` eklenmesi ADR-012'nin öngördüğü genişletme mekanizmasının üçüncü uygulaması, yeni bir mimari karar değil.

Backlog madde #12 ("Sayfa Yönetimi") **tamamen tamamlandı** işaretlendi. Yeni backlog maddesi #23 ("Alan-seviyeli RBAC altyapısı") eklendi.

**Sıradaki iş:** SEO veri sözleşmesi (#4), alan-seviyeli RBAC (#23) veya Bayi/Showroom Yönetimi (#11) — henüz başlanmadı, onay bekliyor.

*(Önceki durum korunuyor: Task 10 — Banner Yönetimi — TAMAMLANDI 19.07.2026, bkz. aşağıdaki detay.)*

## Önceki Task Detayı — Task 10 — Banner Yönetimi (Backlog #13) — TAMAMLANDI (19.07.2026).

**Kritik bulgu 1 — Alan sınıflandırması (Madde 16.1, prose metin):** Tam metin: *"Hero Banner yönetim panelinden yönetilebilir olacaktır: görsel/video yükleme, başlık, alt başlık, CTA butonu metni ve linki, sıralama, aktif/pasif durumu, yayın tarihi aralığı."* Blog/News'ten üç somut noktada bilinçli olarak **farklı** modellendi (körü körüne aynı desen kopyalanmadı):
  - **Sıralama:** Blog/News'te yoktu (kronolojik akış yeterliydi); Banner'da doküman açıkça istiyor → `DisplayOrder` eklendi (Category/Collection/Document/ReferenceProject deseni).
  - **Aktif/pasif durumu:** Blog/News'in 3-durumlu `Status` enum'ı (Taslak/Yayında/Arşiv) yerine, doküman burada yalnızca 2 durum ("aktif/pasif") istediği için **bool `IsActive` + `ToggleActive`** kullanıldı (Category/Collection/ReferenceProject deseni).
  - **Yayın tarihi:** Blog/News'in tekil `PublishDate`'i yerine doküman açıkça "aralık" dediği için **`PublishStartDate`+`PublishEndDate`** çifti (nullable, ikisi de opsiyonel, ama ikisi de doluysa başlangıç≤bitiş validasyonu var).

**Kritik bulgu 2 — Video yükleme bilinçli olarak kapsam dışı bırakıldı:** Doküman "görsel/video yükleme" diyor, ama projenin mevcut dosya-doğrulama altyapısı (magic-byte imzaları — JPEG/PNG/WEBP, MIME whitelist) yalnızca görsel formatları için kurulu. Video desteği ayrı bir format/güvenlik yüzeyi (farklı magic-byte imzaları, muhtemelen çok daha büyük dosya boyutu limitleri, farklı depolama/CDN stratejisi) gerektirir. Bu, kritik bir mimari çelişki ya da veri kaybı riski oluşturmadığı için durdurulmadı — yalnızca görsel desteklendi, video ayrı bir backlog maddesi olarak bırakıldı ve raporda açıkça not düşüldü.

**Kritik bulgu 3 — SEO hiç eklenmedi:** Madde 17.2 ("Banner Yönetimi: Hero banner, kampanya banner, anasayfa bileşenleri") ve Madde 36.1 ("Banners | Banner yönetimi | —") Banner için SEO'yu hiç anmıyor — Blog/News'ten farklı olarak SeoUrl/MetaTitle/MetaDescription Banner'a eklenmedi. Bu tutarlılık Madde 30'un RBAC satırında da yansıyor: SEO Editörü'ne salt-görüntüleme bile verilmedi (bkz. Kritik bulgu 5).

**Kritik bulgu 4 — BannerType taksonomisi icat edilmedi:** Madde 17.2'nin "Hero banner, kampanya banner, anasayfa bileşenleri" ifadesi örnekleyici/tanımlayıcı — DocumentType (Katalog/Teknik Föy/Sertifika/Rapor) veya ReferenceProjectType (Konut/Otel/Ofis/...) gibi açık bir tip listesi verilmiyor. Bu yüzden ayrı bir `BannerType` enum'u icat edilmedi; tek düz `Banner` entity'si tüm senaryoları (hero, kampanya, anasayfa bileşeni) kapsıyor — hangi banner'ın nerede gösterileceği bu fazın (yalnızca admin panel) kapsamı dışında, public site'ın konusu.

**Kritik bulgu 5 — RBAC (Madde 30, Blog/News'ten farklı):** Banner satırı: Admin=Tam, İçerik Editörü=CRUD (silme dahil), SEO Editörü=— (Blog/News'in "salt-görüntüleme"sinden farklı, **hiç erişim yok** — SEO alanı olmadığı için mantıksal olarak tutarlı), Ürün Yöneticisi=—. Yalnızca 2 rolün herhangi bir erişimi olduğu için tek bir `ViewRoles`=`EditRoles` sabiti yeterli oldu (Blog/News'teki ayrı ViewRoles/EditRoles ayrımına gerek kalmadı).

**İlişki yokluğu:** Banner, Category/Tag/Product'tan tamamen bağımsız — projenin şu ana kadarki en yalın CMS modülü (Blog'un BlogCategory+Tag+BlogTag'i, News'in NewsCategory'si gibi hiçbir ilişkisi yok).

**Domain:** `Banner` (`Id`, `ImagePath?`, `PublishStartDate?`, `PublishEndDate?`, `DisplayOrder`, `IsActive` — audit alanı yok). Yeni enum yok. **Yeni EntityType üyesi de gerekmedi** — `EntityType.Banner` Task 3.1B'nin orijinal 9-üyeli enum'unda zaten vardı (Product/Category/Collection/Blog/News/Page/Banner/ReferenceProject/Dealer), ama bugüne kadar hiçbir modül onu tüketmemişti; bu task ilk gerçek kullanımı.

**Application:** `Application/Banners/` — `BannerService` (Category/Collection'ın bool-IsActive+ToggleActive deseni + Blog/News'in tekil-görsel-doğrulama deseni tek serviste birleşti, artı yayın-tarihi-aralığı validasyonu). Mevcut `ITranslationService`/`IUnitOfWork`/`IFileStorageService` **hiç değiştirilmeden** tekrar kullanıldı — Category/Product/Product silme guard'ları gibi ekstra repository metodu da gerekmedi (Banner'ın hiçbir ilişkisi yok).

**Infrastructure:** `BannerConfiguration`, `BannerRepository`.

**Presentation:** `BannerController` (Index/Create/Edit/ToggleActive/Delete — Category/Collection'daki gibi, Blog/News'in aksine `ToggleActive` **var**), görsel Create/Edit'e inline `IFormFile? image` parametresiyle entegre (Blog/News'teki desenin aynısı).

**Migration:** `AddBanner` — yalnızca `Banners`; başka hiçbir tabloya dokunulmadı.

**Doğrulama (33/33 iş kuralı/güvenlik senaryosu ✅):** TR başlık zorunluluğu, negatif sıralama reddi, **yayın tarihi aralığı geçersizliği reddi** (başlangıç>bitiş), geçerli oluşturma (tarih aralığı+CTA ile), minimal (yalnızca başlık) oluşturma, ToggleActive, geçersiz uzantı/MIME-uyuşmazlığı/boyut-aşımı/magic-byte reddi, geçerli görsel yükleme, görsel değiştirme (eski dosya silinir), görsel kaldırma (dosya silinir), upsert+opsiyonel-alan-silme (tarih aralığı null'a çekildi, Subtitle/ButtonText/ButtonUrl boşaltıldı), silme + Translation temizliği — tamamı geçici doğrulama koduyla (`BannerService` doğrudan çağrılarak) test edildi. sqlcmd ile tam temizlik doğrulandı (Banners 0 satır). Identity etkilenmedi. Uygulama gerçekten başlatılıp 5 endpoint'te anonim erişimin Login'e yönlendirdiği curl ile doğrulandı; unhandled exception yok.

Geçici doğrulama kodu (`BannerVerification.cs` + `--verify-banner` bloğu) ve geçici `wwwroot/uploads/banners` test klasörleri tamamen kaldırıldı.

**Yeni ADR gerekmedi** — ADR-006/013/014 deseni altıncı kez tekrar kullanıldı; yeni bir EntityType üyesi de gerekmedi.

Backlog madde #13 ("Banner Yönetimi") **tamamen tamamlandı** işaretlendi.

**Sıradaki iş:** SEO veri sözleşmesi (#4), Sayfa Yönetimi (#12) veya Bayi/Showroom Yönetimi (#11) — henüz başlanmadı, onay bekliyor.

*(Önceki durum korunuyor: Task 9 — Haber Yönetimi — TAMAMLANDI 19.07.2026, bkz. aşağıdaki detay.)*

## Önceki Task Detayı — Task 9 — Haber Yönetimi (Backlog #15) — TAMAMLANDI (19.07.2026).

**Kritik bulgu 1 — Alan sınıflandırması (Madde 22, prose metin, Blog'un 21.1 tablosu gibi ayrı bir alan tablosu yok):** Madde 22'nin tam metni: *"Haber veri modeli blog modülü ile benzer yapıda olacaktır: başlık, içerik, kapak görseli, kategori, yayın tarihi, durum ve SEO alanları."* Blog'un 21.1 tablosundaki **Excerpt, Author, Tags bu listede yok** — kullanıcı talimatı "Dokümanda olmayan alan ekleme" ve "aynı deseni tekrar kullan (ama gereksiz yeni mimari oluşturma)" birlikte değerlendirildi: "aynı deseni kullan" mimari/kod deseni anlamına alındı (Translation-CRUD, tekil görsel doğrulama, SetNull kategori ilişkisi), alan listesine körü körüne genişletme gerekçesi sayılmadı. Title/Content = Translation; NewsCategory/PublishDate/Status/FeaturedImage = native; SEO = Translation. DisplayOrder eklenmedi (Blog ile aynı gerekçe — kronolojik akış, Madde 15.1).

**Kritik bulgu 2 — NewsCategory (Madde 36.1 + Madde 28.2, BlogCategory'nin birebir tekrarı):** Madde 36.1 "NewsCategories"i ayrı bir tablo olarak listeliyor → yeni `NewsCategory` entity, BlogCategory ile aynı desen (düz/hiyerarşisiz, Translation-tabanlı Name — Madde 28.2 "kategori adı" genel çoklu-dil gereksinimi). Yeni `EntityType.NewsCategory` eklendi (enum + `EntityTypeMapping`, migration gerektirmedi — ikinci kez aynı genişletme mekanizması, ilki `EntityType.BlogCategory` idi). Madde 22'nin verdiği 6 sabit kategori adı (Ödüller/Sürdürülebilirlik/Sertifikalar/Kutlamalar/Bültenler/Kurumsal) **bilinçli olarak seed edilmedi** — CategorySeeder/CollectionSeeder emsaliyle tutarlı (gerçek veri girişi Excel Import/elle-girişin konusu, sahte/fabrik edilmiş çeviri verisi üretilmedi).

**Kritik bulgu 3 — Cardinality ve M2M yokluğu:** News↔NewsCategory many-to-one, nullable FK + `SetNull` (Blog↔BlogCategory ile birebir aynı). Blog'un aksine **News'in hiçbir M2M ilişkisi yok** — Madde 22 Haber için ne ürün ne etiket ilişkisi anıyor, bu yüzden `BlogTag` benzeri bir junction tablosu (`NewsTag` gibi) oluşturulmadı; News, Tag/Product'tan tamamen bağımsız, daha yalın bir entity.

**Kritik bulgu 4 — Status değerleri (doküman belirsiz, gerekçeli çıkarım):** Madde 22 Haber için kendi durum değerlerini saymıyor, yalnızca "durum" diyor. "Blog ile benzer yapı" ifadesi esas alınarak Blog.Status'un somut örneği (Taslak/Yayında/Arşiv) News için de kullanıldı — ama proje genelinde enum paylaşımı hiç yapılmadığı için (ProductStatus/BlogStatus/DocumentType/ReferenceProjectType hepsi ayrı, birbirinden bağımsız enum'lar) yeni, bağımsız bir `NewsStatus` enum'u oluşturuldu (BlogStatus ile aynı şekil, farklı tip — kod paylaşımı yerine desen paylaşımı tercih edildi, projenin genel "entity kendi enum'unu sahiplenir" ilkesiyle tutarlı).

**Kritik bulgu 5 — Görsel modeli (Madde 22 "kapak görseli" — tekil, galeri hiç anılmıyor):** Blog ile birebir aynı karar tekrarlandı — ayrı bir `NewsImage` tablosu/entity'si oluşturulmadı, `News.FeaturedImagePath` doğrudan entity'de nullable string. Yükleme/değiştirme/silme güvenliği ProductImageService/BlogService ile birebir aynı (uzantı whitelist, MIME çapraz kontrol, magic-byte, GUID dosya adı, `IFileStorageService` — beşinci modülde sıfır değişiklikle tekrar kullanıldı, klasör `/uploads/news/{NewsId}/{guid}.{uzanti}`).

**Karşılaşılan ve çözülen derleme sorunu:** `namespace Application.News` içinde bare `News` tipi kullanımı (`INewsRepository`'nin dönüş/parametre tipleri, `NewsService.MapToDtoAsync` parametresi, `new News(...)` constructor çağrısı) "'News' is a namespace but is used like a type" (CS0118) hatası verdi — çünkü namespace adı (`Application.News`) ile Domain entity adı (`Domain.Entities.News`) birebir aynı. Çözüm: bu dört noktada `Domain.Entities.News` tam nitelikli adı kullanıldı; kalıcı bir mimari değişiklik değil, yalnızca isim çakışması çözümü (Application.Blogs/Application.ReferenceProjects gibi diğer klasörlerde bu çakışma yaşanmadı çünkü "Blogs"/"ReferenceProjects" klasör adları kendi entity adlarıyla — "Blog"/"ReferenceProject" — birebir aynı değil, tekil/çoğul farkı koruyor; "News" ise İngilizce'de zaten hem tekil hem çoğul, bu yüzden ayrım kaybolup çakışma oluştu).

**SEO kararı:** Blog ile birebir aynı — Translation-gömülü `SeoUrl`/`MetaTitle`/`MetaDescription` (`NewsFields`), Madde 36.1'in ayrı `SeoMeta` polimorfik tablosu kullanılmadı (SEO veri sözleşmesi #4'ün konusu, hâlâ başlanmadı).

**Domain:** `News` (`Id`, `NewsCategoryId?`/`NewsCategory`, `PublishDate?`, `Status` enum, `FeaturedImagePath?` — audit alanı yok, Author/Excerpt/Tags yok), `NewsCategory` (`DisplayOrder`, `IsActive`). `Domain/Enums/NewsStatus.cs` — Draft/Published/Archived.

**Application:** `Application/News/` — `NewsService` (BlogService'in tekil-görsel-doğrulama + Translation-CRUD deseninin doğrudan tekrarı, ama Tag/Product bağımlılığı yok, daha yalın), `NewsCategoryService` (BlogCategoryService ile birebir — global-duplicate-TR-ad deseni). Mevcut `ITranslationService`/`IUnitOfWork`/`IFileStorageService` **hiç değiştirilmeden** tekrar kullanıldı.

**Infrastructure:** `NewsConfiguration` (NewsCategoryId FK `SetNull`, Status `HasConversion<string>()`, table adı literal "News" — Madde 36.1'e sadakat), `NewsCategoryConfiguration`, `NewsRepository`, `NewsCategoryRepository`.

**Presentation:** `NewsController` (Index/Create/Edit/Delete — `ToggleActive` yok, Status enum'la normal Edit formundan değişiyor), `NewsCategoryController` (Index/Create/Edit/ToggleActive/Delete — BlogCategoryController ile birebir), kapak görseli Create/Edit'e inline `IFormFile? featuredImage` parametresiyle entegre edildi (BlogController'daki desenin aynısı, tag input'u yok).

**RBAC:** Madde 30'un "Blog/Haber" satırı **tek satırda birleşik** — BlogController ile birebir aynı sabitler: Admin=Tam, İçerik Editörü=CRUD (silme dahil), SEO Editörü=salt-görüntüleme (Madde 30'un "Meta Alanları" ifadesi, projede hiç uygulanmayan alan-seviyeli RBAC yerine action-seviyeli view-only'e indirgendi — Task 5'ten beri tutarlı), Ürün Yöneticisi=erişim yok. `NewsCategoryController` aynı matrisi kullanıyor.

**Migration:** `AddNews` — yalnızca `News`/`NewsCategories`; başka hiçbir tabloya dokunulmadı.

**Doğrulama (31/31 iş kuralı/güvenlik senaryosu ✅):** NewsCategory TR ad zorunluluğu + global duplicate reddi, News TR başlık zorunluluğu, var olmayan NewsCategoryId reddi, geçerli oluşturma (kategori+tarih+durum ile), minimal (yalnızca başlık) oluşturma, geçersiz uzantı/MIME-uyuşmazlığı/boyut-aşımı/magic-byte reddi, geçerli kapak yükleme, kapak değiştirme (eski dosya silinir), kapak kaldırma (dosya silinir), upsert+opsiyonel-alan-silme (NewsCategoryId null'a çekildi, Content boşaltıldı), **kategori silme → News hayatta kalıyor** (SetNull davranışı doğrulandı), haber silme + Translation temizliği — tamamı geçici doğrulama koduyla (`NewsService`/`NewsCategoryService` doğrudan çağrılarak) test edildi. sqlcmd ile tam temizlik doğrulandı (News/NewsCategories tümü 0 satır — Blog'un Tag havuzu gibi kalıcı bir paylaşılan tablo News'te yok, ekstra manuel temizlik gerekmedi). Identity etkilenmedi. Uygulama gerçekten başlatılıp 7 endpoint'te anonim erişimin Login'e yönlendirdiği curl ile doğrulandı; unhandled exception yok.

Geçici doğrulama kodu (`NewsVerification.cs` + `--verify-news` bloğu) ve geçici `wwwroot/uploads/news` test klasörleri tamamen kaldırıldı.

**Yeni ADR gerekmedi** — ADR-006/013/014 deseni beşinci kez tekrar kullanıldı; `EntityType.NewsCategory` eklenmesi de aynı öngörülen genişletme mekanizmasının ikinci uygulaması, yeni bir mimari karar değil.

Backlog madde #15 ("Haber Yönetimi") **tamamen tamamlandı** işaretlendi.

**Sıradaki iş:** SEO veri sözleşmesi (#4), Banner Yönetimi (#13) veya Bayi/Showroom Yönetimi (#11) — henüz başlanmadı, onay bekliyor.

*(Önceki durum korunuyor: Task 8 — Blog Yönetimi — TAMAMLANDI 19.07.2026, bkz. aşağıdaki detay.)*

## Önceki Task Detayı — Task 8 — Blog Yönetimi (Backlog #14 — kullanıcı isteğinde "#11" denildi, TASKS.md'nin taslak backlog numaralandırmasında Blog #14/Bayi-Showroom #11'dir; modül adı açık olduğu için numaralandırma farkı durdurma nedeni sayılmadı) — TAMAMLANDI (19.07.2026).

**Kritik bulgu 1 — Alan sınıflandırması (Madde 21/21.1, onay istenmeden çözüldü):** Title/Excerpt/Content/Slug/MetaTitle/MetaDescription doküman'da "(multi-lang)" işaretli → Translation'a taşındı (Slug, projede kurulu "SeoUrl" adlandırma konvansiyonuyla tutarlılık için `SeoUrl` alan adı kullanıldı). Author/PublishDate/Status/FeaturedImage "(multi-lang)" işaretli **değil** → native. Tags de "(multi-lang)" işaretli değil (Title/Excerpt/Content'ten bilinçli farklı) → native + M2M. Madde 21.1 tablosunda Product/Document'ın aksine Zorunluluk sütunu yok — yalnızca TR Title zorunlu tutuldu, geri kalan her şey opsiyonel.

**Kritik bulgu 2 — Kategori ilişkisi (Madde 36.1 + Madde 28.2 ile çözüldü, tahmin gerekmedi):** Madde 36.1 "BlogCategories"i Product'ın "Categories" tablosundan **ayrı** bir tablo olarak listeliyor — bu yüzden mevcut `Category` entity'si (ürün/ceramik sınıflandırması) hiç dokunulmadan, yeni bağımsız `BlogCategory` entity'si oluşturuldu (düz/hiyerarşisiz, Product'ın 2-seviyeli ağacından farklı — doküman blog kategorileri için hiyerarşi işareti vermiyor). Madde 28.2 "kategori adı"nı **genel olarak** (ürün/blog ayrımı yapmadan) çoklu dil gerektiren alanlar arasında saydığı için `BlogCategory.Name` de Translation'a taşındı — yeni `EntityType.BlogCategory` eklendi (`Domain/Enums/EntityType.cs` + `EntityTypeMapping.cs`, migration **gerektirmedi**, Translations.EntityType zaten esnek nvarchar). Cardinality: Blog↔BlogCategory many-to-one, **nullable FK + SetNull** (Category/Collection'ın zorunlu-FK Restrict deseninden bilinçli farklı — Blog.BlogCategoryId doğası gereği opsiyonel, kategori silinirse blog etkilenmez, ekstra guard kodu gerekmedi).

**Kritik bulgu 3 — Etiket ilişkisi:** "Tags: array" many-to-many (`Tag`/`BlogTag`, ProductDocument/ProductReferenceProject ile aynı junction deseni). Tag native (Translation yok), paylaşılan/tekrar-kullanılabilir bir havuz — case-insensitive unique index + get-or-create mantığıyla aynı etiketin ikinci bir satırı oluşmuyor. Blog silindiğinde Tag satırları **silinmiyor** (bilinçli tasarım — doküman ayrı bir etiket yönetim/silme ekranı istemiyor, tag cloud mantığıyla tutarlı).

**Kritik bulgu 4 — Görsel modeli (Madde 21.1 "FeaturedImage: image" — tekil, galeri değil, kullanıcı talimatıyla da teyitli):** ProductImage/ReferenceProjectImage gibi ayrı bir tablo/entity **oluşturulmadı** — kullanıcı zaten "ProductImage veya ReferenceProjectImage'ı doğrudan kullanma" demişti, ama doküman da zaten yalnızca tekil bir "image" istiyor (ReferenceProject'in "gallery" ayrımı burada yok). `Blog.FeaturedImagePath` doğrudan entity'de nullable string olarak tutuldu — ancak yükleme/değiştirme/silme güvenliği ProductImageService ile **birebir aynı**: uzantı whitelist (.jpg/.jpeg/.png/.webp), MIME çapraz kontrol, magic-byte imza doğrulama, GUID dosya adı, `IFileStorageService` (dördüncü modülde sıfır değişiklikle tekrar kullanıldı), path traversal koruması (LocalFileStorageService'in kendi korumasıyla), DB hatasında fiziksel dosya geri alma, değiştirme/silme/blog-silme'de eski dosyanın temizliği.

**SEO kararı:** Mevcut Translation-gömülü SEO deseni (Product/Category/Collection/ReferenceProject ile birebir — `SeoUrl`/`MetaTitle`/`MetaDescription` alan sabitleri `BlogFields`'te) korundu; Madde 36.1'in ayrı `SeoMeta` polimorfik tablosu **kullanılmadı** — farklı bir SEO sistemi icat edilmedi, SEO veri sözleşmesi (#4) task'ı hâlâ ayrı ve başlanmamış.

**Domain:** `Blog` (`Id`, `BlogCategoryId?`/`BlogCategory`, `Author?`, `PublishDate?`, `Status` enum, `FeaturedImagePath?` — audit alanı yok, DisplayOrder yok [Madde 15.1 "son eklenen" → PublishDate'e göre kronolojik, manuel sıralama istenmiyor]), `BlogCategory` (`DisplayOrder`, `IsActive`), `Tag` (`Name`, unique), `BlogTag` (junction). `Domain/Enums/BlogStatus.cs` — Draft/Published/Archived (Madde 21.1'in kendi 3 durumu, gerçek enum — Category/Collection/ReferenceProject'in bool `IsActive`'inden bilinçli farklı, Product.Status emsali).

**Application:** `Application/Blogs/` — `BlogService` (Category/Collection'ın Translation-CRUD deseni + Document'ın M2M-replace deseni [`ReplaceTagsAsync`] + ProductImage'ın tekil-görsel doğrulama deseni tek serviste birleşti), `BlogCategoryService` (Collection'ın global-duplicate-TR-ad deseniyle birebir), `ITagRepository`/`TagRepository` (get-or-create, `GetByNameAsync`). Mevcut `ITranslationService`/`IUnitOfWork`/`IFileStorageService`/`IProductRepository` (kullanılmadı, Blog'un Product ile hiçbir ilişkisi yok) **hiç değiştirilmeden** tekrar kullanıldı.

**Infrastructure:** `BlogConfiguration` (BlogCategoryId FK `SetNull`, Status `HasConversion<string>()`, FeaturedImagePath nvarchar(500)), `BlogCategoryConfiguration`, `TagConfiguration` (unique index Name), `BlogTagConfiguration` (Cascade + composite unique index), `BlogRepository`, `BlogCategoryRepository`, `TagRepository`.

**Presentation:** `BlogController` (Index/Create/Edit/Delete — `ToggleActive` yok, Status enum'la normal Edit formundan değişiyor, Product'taki gibi), `BlogCategoryController` (Index/Create/Edit/ToggleActive/Delete — Collection'daki gibi), kapak görseli Create/Edit'e inline `IFormFile? featuredImage` parametresiyle entegre edildi (DocumentController'ın dosya-değiştirme desenine benzer, ama tek dosya/tek alan — ayrı bir BlogImageController **yok**), etiketler tek bir virgülle-ayrılmış metin kutusundan (`TagsInput`) serbest metin girişiyle.

**RBAC:** Madde 30'un Blog/Haber satırı **literal** uygulandı (bu kez tablo da var — ReferenceProject'teki gibi Madde 7.2'ye dayanan çıkarım gerekmedi): Admin=Tam, İçerik Editörü=CRUD (silme dahil, tablo bu ifadeyi kullanıyor), SEO Editörü=salt-görüntüleme (Madde 30'un "Meta Alanları" ifadesi, projede hiç uygulanmayan alan-seviyeli RBAC yerine action-seviyeli view-only'e indirgendi — Task 5'ten beri tutarlı bir proje kararı), Ürün Yöneticisi=erişim yok. `BlogCategoryController` aynı matrisi kullanıyor (Madde 17.2 "kategori"yi Blog Yönetimi'nin kendi fonksiyonu olarak sayıyor, ayrı bir RBAC satırı yok).

**Migration:** `AddBlog` — yalnızca `Blogs`/`BlogCategories`/`Tags`/`BlogTags`; Identity/Languages/Translations/Categories/Collections/Products/ProductImages/Documents/ProductDocuments/CollectionDocuments/ReferenceProjects/ReferenceProjectImages/ProductReferenceProjects'a dokunulmadı.

**Doğrulama (35/35 iş kuralı/güvenlik senaryosu ✅):** BlogCategory TR ad zorunluluğu + global duplicate reddi, Blog TR başlık zorunluluğu, var olmayan BlogCategoryId reddi, geçerli oluşturma (kategori+yazar+tarih+durum+etiketler ile), **case-insensitive etiket tekilleştirme** ("Banyo"/"banyo" tek etikete indirgendi), minimal (yalnızca başlık) oluşturma, geçersiz uzantı/MIME-uyuşmazlığı/boyut-aşımı/magic-byte reddi, geçerli kapak yükleme, kapak değiştirme (eski dosya silinir), kapak kaldırma (dosya silinir), upsert+opsiyonel-alan-silme (Author/Excerpt boşaltıldı, BlogCategoryId null'a çekildi, etiketler değiştirildi), **kategori silme → Blog hayatta kalıyor** (SetNull davranışı doğrulandı), blog silme + Translation temizliği — tamamı geçici doğrulama koduyla (`BlogService`/`BlogCategoryService` doğrudan çağrılarak) test edildi. sqlcmd ile tam temizlik doğrulandı (Blogs/BlogCategories/BlogTags tümü 0 satır; Tags havuzunda kalan 3 test etiketi bilinçli tasarım gereği otomatik silinmediği için manuel sqlcmd ile temizlendi). Identity etkilenmedi. Uygulama gerçekten başlatılıp 7 endpoint'te anonim erişimin Login'e yönlendirdiği curl ile doğrulandı; unhandled exception yok.

Geçici doğrulama kodu (`BlogVerification.cs` + `--verify-blog` bloğu) ve geçici `wwwroot/uploads/blog` test klasörleri tamamen kaldırıldı.

**Yeni ADR gerekmedi** — ADR-006/013/014 deseni dördüncü kez tekrar kullanıldı; `EntityType.BlogCategory` eklenmesi Task 3.1B'nin öngördüğü genişletme mekanizmasının (yeni polimorfik tip = yeni enum üyesi) ilk gerçek kullanımı, yeni bir mimari karar değil.

Backlog madde ("Blog Yönetimi") **tamamen tamamlandı** işaretlendi.

**Sıradaki iş:** SEO veri sözleşmesi (#4), Haber Yönetimi (#15) veya Bayi/Showroom Yönetimi (#11) — henüz başlanmadı, onay bekliyor.

*(Önceki durum korunuyor: Task 7 — Referans Proje Yönetimi — TAMAMLANDI 19.07.2026, bkz. aşağıdaki detay.)*

## Önceki Task Detayı — Task 7 — Referans Proje Yönetimi (Backlog #10, uçtan uca) — TAMAMLANDI (19.07.2026).

**Kritik bulgu 1 — RBAC (Madde 30 tabloda satır yok, Madde 7.2 ile çözüldü, onay istenmedi):** Madde 30'un yetkilendirme tablosu Referans Proje Yönetimi'ni hiç listelemiyor (Dashboard/Sayfa/Ürün/Blog-Haber/Banner/Katalog-Doküman/Bayi-Showroom/Form/SEO/Dil/Kullanıcı/Excel Import var, Referans Proje yok). Madde 7.2 (Roller) İçerik Editörü'nün yetki kapsamını "Sayfa, blog, haber, banner, **referans proje**, ürün açıklamaları (onay akışı ile)" olarak açıkça sayıyor. Bu nedenle Blog/Haber/Banner ile birebir aynı satır uygulandı: Admin=Tam, İçerik Editörü=CRUD (silme dahil — tablo bu üç modül için "CRUD" diyor, önceki modüllerin kendi bespoke "silme yalnızca Admin" kısıtlaması buraya taşınmadı), SEO Editörü=salt-görüntüleme (Category/Collection/Product'taki gibi — alan-seviyeli RBAC hâlâ hiçbir modülde uygulanmıyor), Ürün Yöneticisi=erişim yok (Madde 7.2'nin Ürün Yöneticisi tanımı "Ürün ekleme/düzenleme, Excel import, görsel yükleme, doküman ilişkilendirme" — referans projeyi kapsamıyor).

**Kritik bulgu 2 — Cardinality (Madde 23/23.1/36.1/36.2'de doğrudan yazılı, tahmin gerekmedi):** "Bir ürün birden fazla projede kullanılabilir. Bir proje birden fazla ürün içerebilir" — `ReferenceProject`↔`Product` many-to-many. Madde 36.2 junction'ı "ProductProjects" adıyla anıyor; burada `ReferenceProject` adlandırmasıyla (EntityType enum'da Task 3.1B'den beri mevcut) tutarlılık için `ProductReferenceProject` adı kullanıldı. Cascade FK her iki yönde — Product veya ReferenceProject silinirse yalnızca ilişki satırı gider (Document/ProductDocument ile aynı desen). Restrict FK hiçbir yönde yok, bu yüzden Task 6'daki gibi bir silme-guard yan-bulgusu bu task'ta **oluşmadı**.

**Kritik bulgu 3 — Görsel modeli (Madde 23.1 "Images: gallery" + "FeaturedImage: image", onay istenmeden çözüldü):** ProductImage'ın 5 tipli (Render/Face/Lifestyle/Texture/Detail) modeli **tekrar kullanılmadı** — doküman Referans Proje için tip ayrımı istemiyor, yalnızca galeri + tek kapak görseli istiyor. Bunun yerine ProductImage'ın `IsPrimary` desenindeki gibi tek `IsFeatured` bayrağıyla galeri+kapak tek tabloda (`ReferenceProjectImage`) birleştirildi — ayrı bir `FeaturedImage` sütunu/tablosu icat edilmedi (ADR-013'ün deseni, iki farklı storage mekanizması yerine). Proje kuralı gereği ("ProductImage yalnızca Product'a özeldir, başka entity gerekiyorsa kendi entity'sini oluşturacak") `ReferenceProjectImage` sıfırdan, `ProductImage`'a dokunmadan eklendi. Klasör anahtarı ADR-014'teki gibi sapıyor: `/uploads/projects/{ReferenceProjectId}/{guid}.{uzanti}` — `ProductCode` gibi doğal bir iş kodu ReferenceProject'te olmadığı için surrogate `Id` kullanıldı.

**Kritik bulgu 4 — Zorunluluk sütunu eksikliği (doküman farkı, MVP kararı):** Madde 23.1'in Proje Veri Modeli tablosu — Product/Document'ın aksine — hiçbir "Zorunluluk" sütunu içermiyor (yalnızca Alan/Tip/Açıklama). Müşteri Notu ("Şu an referanslara ait bir arşiv bulunmadığından ileriki fazlarda içerik eklenecektir, ancak altyapı ve kurgu ilk fazda hazır olmalıdır") esas alınarak yalnızca TR `ProjectName` + `ProjectType` zorunlu tutuldu; `Location`/`Architect`/`Year`/görseller/ilişkili ürünler tamamen opsiyonel — böylece "boş arşiv" ile de proje oluşturulabiliyor (doğrulama testinde bizzat bu senaryo test edildi: yalnızca ad+tip ile oluşturma).

**Domain:** `ReferenceProject` (`Id`, `Location?`, `ProjectType` enum, `Architect?`, `Year?`, `DisplayOrder`, `IsActive` — audit alanı yok, Category/Collection/Document ile tutarlı), `ReferenceProjectImage` (`ReferenceProjectId`/`ReferenceProject` FK Cascade, `FilePath`, `IsFeatured`, `DisplayOrder`), `ProductReferenceProject` (junction, sade POCO). `Domain/Enums/ReferenceProjectType.cs` — 6 üye (Konut/Otel/Ofis/Avm/Hastane/DisMekan), Madde 23.1'in "vb." ekini içermiyor (EntityType'taki gibi spekülatif üye eklenmedi).

**Application:** `Application/ReferenceProjects/` (`IReferenceProjectRepository`, `ReferenceProjectService`, `ReferenceProjectDto`, `ReferenceProjectRequests`, `ReferenceProjectOperationResult`, `ReferenceProjectEnumDisplay`, `ReferenceProjectFields`) — Category/Collection'ın Translation-CRUD deseniyle + Document'ın M2M-ilişki-replace deseniyle birebir. `IReferenceProjectImageRepository`/`ReferenceProjectImageService` — `ProductImageService` ile birebir aynı desen (boyut/uzantı/MIME/magic-byte doğrulama, ilk-yükleme-otomatik-kapak, silme fallback'i, DB hatasında fiziksel dosya geri alma). Mevcut `ITranslationService`/`IUnitOfWork`/`IProductRepository`/`IFileStorageService` **hiç değiştirilmeden** tekrar kullanıldı.

**Infrastructure:** `ReferenceProjectConfiguration`, `ReferenceProjectImageConfiguration` (FK Cascade, filtered unique index `WHERE [IsFeatured]=1` — ProductImage'daki desenin tekrarı), `ProductReferenceProjectConfiguration` (Cascade + composite unique index), `ReferenceProjectRepository`, `ReferenceProjectImageRepository`.

**Presentation:** `ReferenceProjectController` (Index/Create/Edit/ToggleActive/Delete), ayrı `ReferenceProjectImageController` (Upload/SetFeatured/MoveUp/MoveDown/Delete — Product Edit'teki "Görseller" bölümü deseninin aynısı), Ürün çoklu-seçim (`<select multiple>`), ProjectType dropdown, Çeviri alanları (ProjectName/Description/SeoUrl).

**Migration:** `AddReferenceProjects` — yalnızca `ReferenceProjects`/`ReferenceProjectImages`/`ProductReferenceProjects`; Identity/Languages/Translations/Categories/Collections/Products/ProductImages/Documents/ProductDocuments/CollectionDocuments'a dokunulmadı.

**Doğrulama (38/38 iş kuralı/güvenlik senaryosu ✅):** TR proje adı zorunluluğu, negatif sıralama reddi, var olmayan ürün ilişkisi reddi, geçerli oluşturma (Location/Architect/Year/2-ürün-ilişkisi ile), **minimal ("boş arşiv") oluşturma — yalnızca ad+tip ile**, upsert+opsiyonel-alan-silme (Architect boşaltıldı, ilişki 1 ürüne düşürüldü, Description silindi), toggle active, geçersiz uzantı/MIME-uyuşmazlığı/boş-dosya/boyut-aşımı/magic-byte reddi, var olmayan projeye yükleme reddi, ilk-yükleme-otomatik-kapak, ikinci görsel kapak değil, SetFeatured tekilliği, MoveUp sıralama, çapraz-proje izolasyonu, silme+fallback-kapak-ataması, **ürün silme → ReferenceProject hayatta kalıyor** (Document'takiyle aynı kritik davranış), proje silme → görsel DB+disk temizliği + Translation temizliği — tamamı geçici doğrulama koduyla (`ReferenceProjectService`/`ReferenceProjectImageService` doğrudan çağrılarak) test edildi. sqlcmd ile tam temizlik doğrulandı (ReferenceProjects/ReferenceProjectImages/ProductReferenceProjects/Products/Categories/Collections/Translations tümü 0 satır, Identity etkilenmedi). Uygulama gerçekten başlatılıp 5 endpoint'te anonim erişimin Login'e yönlendirdiği ve rol yokken RBAC challenge'ının (302) çalıştığı curl ile doğrulandı; unhandled exception yok.

Geçici doğrulama kodu (`ReferenceProjectVerification.cs` + `--verify-reference-projects` bloğu) ve geçici `wwwroot/uploads/projects` test klasörleri tamamen kaldırıldı.

**Yeni ADR gerekmedi** — ADR-006/ADR-013/ADR-014 deseni hiçbir yeni genel-amaçlı abstraction eklenmeden üçüncü kez tekrar kullanıldı.

Backlog madde #10 ("Referans Proje Yönetimi") **tamamen tamamlandı** işaretlendi.

**Sıradaki iş:** SEO veri sözleşmesi (#4) veya Bayi/Showroom Yönetimi (#11) — henüz başlanmadı, onay bekliyor.

*(Önceki durum korunuyor: Task 6 — Katalog/Doküman Yönetimi — TAMAMLANDI 19.07.2026, bkz. aşağıdaki detay.)*

## Önceki Task Detayı — Task 6 — Katalog / Doküman Yönetimi (Backlog #9, uçtan uca) — TAMAMLANDI (19.07.2026).

**Kritik bulgu 1 — Çoklu dil (Translation KULLANILMADI, doküman-içi tutarlılıkla çözüldü, onay istenmedi):** Madde 24'ün `Document` tablosu hem `DocumentName`'i "string(multi-lang)" işaretliyor hem ayrı bir `Language` (enum) alanı listeliyor — görünürde çelişkili. Madde 18.3'ün dosya isimlendirme standardı ("...dokumanTipi_dil.pdf", örn. `_teknik-foy_tr.pdf`) çözümü veriyor: **her fiziksel PDF tek bir dile ait** — TR/EN sürümleri aynı satırın çevirisi değil, **iki ayrı `Document` satırı/iki ayrı fiziksel dosya**. Bu yüzden `Document.Title` **Translation'a taşınmadı**, native sütun; `Document.LanguageId` (mevcut `Language` entity'sine FK) satırın dilini belirtiyor. `EntityType.Document` eklenmedi.

**Kritik bulgu 2 — İlişki modeli (Madde 36.1/36.2'de doğrudan yazılı, tahmin gerekmedi):** `Document`, `Product`/`Collection` ile **many-to-many** — doküman bunu `ProductDocuments`/`CollectionDocuments` junction tablo adlarıyla birebir belirtiyor. Doküman ayrıca "genel seviyede" (ilişkisiz) doküman olabileceğini açıkça söylüyor (Madde 24). `Document` üzerinde doğrudan `ProductId`/`CollectionId` yok.

**Kritik bulgu 3 — Silme davranışı ve klasör yapısı sapması (ADR-014 olarak kaydedildi):** `ProductDocument`/`CollectionDocument` FK'leri Product/Collection tarafında Cascade — bir ürün/koleksiyon silindiğinde yalnızca ilişki satırı gider, **Document ve fiziksel dosya etkilenmez** (paylaşılan/genel dokümanlar için doğru davranış). ADR-006/Madde 35.4'ün literal `/products/{urunKodu}/documents,...` klasör örneği, Document'ın M2M+opsiyonel cardinality'si nedeniyle yapısal olarak uygulanamadı (hangi ürün "sahip" seçilecek? ilişkisiz dokümanlar nereye gidecek?) — bunun yerine `/uploads/documents/{tipSegmenti}/{guid}.pdf` kullanıldı (ADR-006'nın ilkesi korunuyor, yalnızca product-code öneki atlandı). Gerekçe ayrıntılı olarak ARCHITECTURE_DECISIONS.md ADR-014'te.

**Yan-bulgu ve düzeltme (bu task sırasında keşfedildi, kapsam dışı ama küçük/geri-dönüşü-kolay olduğu için düzeltildi):** `CategoryService.DeleteAsync`/`CollectionService.DeleteAsync`, Task 5'te eklenen `Product.CategoryId`/`CollectionId` (Restrict FK) referanslarını kontrol etmiyordu — hâlâ bir ürün tarafından kullanılan kategori/koleksiyon silinmeye çalışıldığında **uygulama çöküyordu** (EF Core değişiklik-izleyicisi hatası). `IProductRepository.HasAnyWithCategoryIdAsync`/`HasAnyWithCollectionIdAsync` eklendi, her iki servis artık silmeden önce kontrol edip anlaşılır hata mesajıyla reddediyor. Doğrulama testinde bizzat tetiklenip düzeltmesi de test edildi.

**Domain:** `Document` (Id, DocumentType enum, Title, LanguageId/Language, FilePath, OriginalFileName, FileExtension, ContentType, FileSize, DisplayOrder, IsActive — audit alanı yok, doküman istemiyor), `ProductDocument`/`CollectionDocument` (junction entity'ler, sade POCO).

**Application:** `Application/Documents/` (`IDocumentRepository`, `DocumentService`, `DocumentDto`, `DocumentRequests`, `DocumentOperationResult`, `DocumentEnumDisplay`) — PDF-özel doğrulama (uzantı/MIME/magic-byte `%PDF-`/boyut≤20MB), ilişki yönetimi (`ReplaceProductRelationsAsync`/`ReplaceCollectionRelationsAsync` — diff-based upsert), telafi mantığı (ProductImage ile birebir desen). Mevcut `IFileStorageService`/`LocalFileStorageService` (Task 5.1) **hiç değiştirilmeden** tekrar kullanıldı — arayüz zaten tamamen generic'ti, bu ADR-013'ün öngörüsünü doğruladı.

**Infrastructure:** `DocumentConfiguration` (LanguageId FK Restrict), `ProductDocumentConfiguration`/`CollectionDocumentConfiguration` (Cascade FK'ler + composite unique index), `DocumentRepository`.

**Presentation:** `DocumentController` (Index/Create/Edit/ToggleActive/Delete), Product/Collection çoklu-seçim, Dil/DocumentType dropdown'ları.

**RBAC — Madde 30'a literal sadakat (Task 5/5.1'den farklı, gerekçeli):** Doküman bu modül için açıkça satır içeriyor: Admin=Tam, İçerik Editörü=**Yükleme**, SEO Editörü=—, Ürün Yöneticisi=Tam. Action-seviyeli ayrım doğrudan uygulanabilir olduğu için (Create ayrı action) hayata geçirildi: İçerik Editörü yalnızca Create'e erişebiliyor; SEO Editörü modüle hiç erişemiyor (diğer modüllerin aksine salt-görüntüleme bile yok); Ürün Yöneticisi'ne "Tam" dendiği için silme yetkisi de verildi (önceki modüllerin "silme yalnızca Admin" kendi kararından bilinçli sapma).

**Migration:** `AddDocuments` — yalnızca `Documents`/`ProductDocuments`/`CollectionDocuments`; Identity/Languages/Translations/Categories/Collections/Products/ProductImages'a dokunulmadı.

**Doğrulama:** Build 0/0; migration içeriği yalnızca beklenen şemayı içeriyor; **54/54** iş kuralı/güvenlik senaryosu geçti (geçerli PDF+ilişkiler, geçersiz uzantı/MIME/magic-byte/boş/boyut-aşımı reddi, Title/Language/DisplayOrder/ilişki-var-olma doğrulaması, genel-ilişkisiz doküman, metadata-only edit, dosya değiştirme+eski-dosya-temizliği, toggle, **Product/Collection silme → Document hayatta kalıyor + fiziksel dosya korunuyor** (kritik davranış), çapraz-doküman izolasyonu, silme DB+disk temizliği, **CategoryService/CollectionService silme-guard düzeltmesi**); sqlcmd ile tam temizlik doğrulandı; 6 endpoint'te anonim erişim engeli curl ile doğrulandı; uygulama normal başladı.

**Sıradaki iş:** Referans Proje Yönetimi (#10) veya SEO veri sözleşmesi (#4) — henüz başlanmadı, onay bekliyor. #10 başladığında `IFileStorageService` + Document'ın M2M/junction-tablo deseni (ReferenceProject↔Product de muhtemelen M2M) doğrudan örnek alınabilir.

*(Önceki durum korunuyor: Task 5.1 — Ürün Görselleri Yönetimi — TAMAMLANDI 19.07.2026, bkz. aşağıdaki detay.)*

## Önceki Task Detayı — Task 5.1 — Ürün Görselleri Yönetimi (Backlog #8, uçtan uca) — TAMAMLANDI (19.07.2026).

**Kritik bulgu (implementasyona başlamadan önce netleşti, migration riskini ortadan kaldırdı):** `Product` entity'sinde (Task 5) hiç `ImagePath` alanı **yoktu** — Madde 18.1'in kendi veri modeli tablosu Product için böyle bir alan listelemiyor, görseller baştan Madde 18.2'nin ayrı çoklu-görsel modeline bırakılmıştı. Bu, "mevcut ImagePath alanını koru mu / kaldır mı" ikilemini tamamen ortadan kaldırdı — `ProductImage` sıfırdan, hiçbir legacy alanla çakışmadan eklendi.

**Kesin gereksinim/belirsiz/dokümanda-yok ayrımı (özet):**
- **Kesin gereksinim:** Madde 18.2'nin 5 görsel tipi (Render/Face/Lifestyle/Texture/Detail) — bu task'ın orijinal talimatındaki basitleştirilmiş "ana görsel+galeri" çerçevesinden daha zengin, ama dokümana sadakat için `ImageType` enum'u olarak eklendi. Görsel sıralaması (dosya isimlendirme standardının `_siraNo` kısmı, Ek-3) → `DisplayOrder`. Ürün silindiğinde görsel temizliği → talimatla + mantıksal zorunlulukla kesinleşti.
- **Belirsiz/MVP kararı:** Tek ana görsel (IsPrimary) — doküman açıkça istemiyor ama Madde 15.3'ün hero-görsel vurgusu + admin liste thumbnail ihtiyacı için eklendi. Format/boyut sınırı (jpg/jpeg/png/webp, 5 MB) — doküman sınır vermiyor, gerekçeli MVP kararı.
- **Dokümanda yok, eklenmedi:** Alt metin/başlık alanı, çoklu dil metadata, video desteği, görsel sayısı sınırı.

**Storage kararı — ADR-006'nın ilk somutlaşması (ADR-013 olarak kaydedildi):** `Application/Storage/IFileStorageService` (Stream-tabanlı, `IFormFile`'a bağımlı değil) + `Infrastructure/Storage/LocalFileStorageService` (wwwroot/uploads, path traversal koruması). Klasör yapısı Madde 35.4/ADR-006'ya **literal sadakatle** uygulandı: `/uploads/products/{ProductCode}/{gorselTipi}/{guid}.{uzanti}` — `ProductId` değil `ProductCode` kullanıldı (ADR ile tutarlılık); `ProductCode` sonradan değişirse eski görseller eski klasör adını korur ama **veri kaybı/kırık link oluşmaz** (DB'de tam yol saklanıyor, yeniden hesaplanmıyor) — kabul edilen kozmetik risk.

**Domain:** `ProductImage` (`Id`, `ProductId`/`Product`, `ImageType` enum, `FilePath`, `IsPrimary`, `DisplayOrder`) — `Product` entity'sine **hiçbir değişiklik yapılmadı** (geri-navigasyon koleksiyonu bile eklenmedi, `Category`/`Collection` ile Product ilişkisindeki `WithMany()` deseniyle birebir tutarlı).

**Application:** `Application/ProductImages/` (`IProductImageRepository`, `ProductImageService`, `ProductImageDto`, `ProductImageOperationResult`, `AddProductImageRequest`, `ProductImageEnumDisplay`) + `Application/Storage/IFileStorageService`. `ProductImageService` içinde: boyut/uzantı/MIME/magic-byte doğrulama (üçüncü parti kütüphane yok, saf BCL byte karşılaştırması), ana-görsel mantığı (ilk yükleme otomatik primary, silme fallback, SetPrimary eskisini otomatik kaldırır), sahiplik kontrolü (imageId productId'ye ait değilse reddedilir), başarısız DB yazımında fiziksel dosya geri alma (compensating action). `ProductService`'e `IProductImageRepository` (liste thumbnail'i için `PrimaryImagePath`) ve `ProductImageService` (silme kaskadı için) enjekte edildi — **hiçbir yeni genel-amaçlı abstraction eklenmeden** mevcut `IUnitOfWork` tekrar kullanıldı.

**Infrastructure:** `LocalFileStorageService`, `ProductImageConfiguration` (FK `Cascade`, `ImageType` `HasConversion<string>()`, **filtered unique index** `WHERE [IsPrimary]=1` — projenin ilk filtered unique index kullanımı), `ProductImageRepository`.

**Presentation:** Ayrı `ProductImageController` (Upload/SetPrimary/MoveUp/MoveDown/Delete — hepsi Admin+Ürün Yöneticisi'ne kapalı, `ProductController`'a aşırı action yığılmadı), Product Edit ekranına "Görseller" bölümü (multipart upload formu + thumbnail listesi + ana-görsel/sıralama/silme aksiyonları), Product Index'e thumbnail sütunu.

**RBAC — bilinçli sınırlama korundu:** Madde 30'un İçerik Editörü'ne görsel düzenleme yetkisi verme isteği bu task'ta da **uygulanmadı** — Task 5'teki karar sessizce genişletilmedi, aynı gerekçeyle (alan-seviyeli RBAC ayrı backlog maddesi) korundu.

**Migration:** `AddProductImages` — yalnızca `ProductImages` tablosu + filtered unique index + `Products`'a Cascade FK; Identity/Languages/Translations/Categories/Collections/Products'a dokunulmadı (Product'ta zaten kaldırılacak bir `ImagePath` olmadığı için beklenen sıfır yan etki doğrulandı).

**Doğrulama:** Build 0/0; migration içeriği yalnızca beklenen şemayı içeriyor; 34/34 iş kuralı/güvenlik senaryosu geçici doğrulama koduyla test edildi (geçerli jpg/png/webp yükleme, geçersiz uzantı/MIME-uyuşmazlığı/boş-dosya/boyut-aşımı/magic-byte-uyuşmazlığı reddi, var olmayan ürün reddi, ana görsel otomasyonu, ana-görsel-tekilliği, sıralama, çapraz-ürün-izolasyonu, silme+fiziksel-dosya-temizliği, ana-görsel-silme-fallback'i, ürün silme kaskadı — hem DB hem disk); sqlcmd ile şema + tam temizlik doğrulandı; anonim erişim engeli 6 endpoint'te curl ile doğrulandı; uygulama normal başladı (unhandled exception yok). Rol-bazlı canlı çoklu-kullanıcı testi, önceki modüllerle aynı credential kısıtı nedeniyle yapılamadı.

**Sıradaki iş:** Katalog/Doküman Yönetimi (#9), Referans Proje Yönetimi (#10) veya SEO veri sözleşmesi (#4) — henüz başlanmadı, onay bekliyor. #9/#10 başladığında bu task'ta kurulan `IFileStorageService`/ADR-013 deseni doğrudan tekrar kullanılabilir.

*(Önceki durum korunuyor: Task 5 — Ürün Yönetimi çekirdek CRUD — TAMAMLANDI 19.07.2026, bkz. aşağıdaki detay.)*

## Önceki Task Detayı — Task 5 — Ürün Yönetimi (çekirdek CRUD, uçtan uca) — TAMAMLANDI (19.07.2026).

**Kapsam sınırlaması (bilinçli, TASKS.md backlog'uyla tutarlı):** Doküman Madde 16.4/17.2, Ürün Yönetimi'ni "projenin en kapsamlı modülü" olarak tanımlıyor (görseller, dokümanlar, ilgili ürünler/projeler dahil). Backlog bu genişliği zaten ayrı maddelere bölmüştü: #7 Ürün Yönetimi (bu task), #8 Ürün Görselleri, #9 Katalog/Doküman Yönetimi, #10 Referans Proje Yönetimi. Bu task yalnızca **çekirdek Product entity'sini** (Madde 18.1'deki veri modeli) kapsar — gerçek dosya yükleme, Document/Project ilişkileri (many-to-many) bu task'ta **yok**, sırası geldiğinde ayrı task'larda eklenecek.

**Kritik ilişki kararı (SeriesName ↔ Collection, onay istenmedi, doküman analiziyle çözüldü):** Madde 18.1'in Ürün Veri Modeli tablosu hem `SeriesName` (Zorunlu, string, "Örn: AMAZONIT, ATLANTIS") hem de ayrı bir `CollectionName` (Opsiyonel, string, "Koleksiyon adı") alanı listeliyor — ikisi de düz metin. Ancak Madde 20 (Koleksiyon Yönetimi) açıkça "Koleksiyonlar seri bazlı gruplandırılacaktır... Her seri bir koleksiyon olarak değerlendirilebilir" diyor ve mevcut veride "272 benzersiz seri adı" = Task 4.2'de kurulan 272 potansiyel `Collection` kaydı ile birebir örtüşüyor. Bu nedenle **`SeriesName`, `Collection` entity'sine FK olarak modellendi** (`Product.CollectionId`, zorunlu); doküman'daki ayrı `CollectionName` serbest metin alanı **eklenmedi** — Collection ilişkisiyle anlamsal olarak çakışan, doğrulanamayan ikinci bir metin alanı YAGNI'ye aykırı olurdu ve doküman bu ikinci alanın nasıl kullanılacağını başka hiçbir yerde açıklamıyor. `Category` alanı da benzer şekilde `Product.CategoryId` FK'sine bağlandı (zorunlu). PROGRESS.md'nin önceki oturumda bıraktığı not ("Product hem CategoryId hem CollectionId FK'sini ayrı ayrı taşıyacak, biri diğerinin üzerinden erişilmeyecek") ile birebir tutarlı — `Collection`'ın `Category`'ye FK'si olmadığı Task 4.2 kararı korunuyor, Product ikisine de bağımsız bağlanıyor.

**Domain:** `Product` (`Domain/Entities/Product.cs`) — 28 native alan: `ProductCode` (zorunlu, unique), `CategoryId`/`Category` (FK, zorunlu), `CollectionId`/`Collection` (FK, zorunlu), `Brand` (enum: NgSeramik/NgStone/NgSlim/NgPerforma), `Status` (enum: Active/Inactive/InProgress/Cancelled — Category/Collection'ın `IsActive bool`'undan farklı, doküman 4 durumlu enum istiyor), `Size`, `Unit`, `Surface`, `Relief`, `SpecialSurface`, `FaceCount`, `Thickness`, `BodyType`, `Color`, `ColorMaterial`, `ApplicationArea`, `UsageArea`, `Finish`, `PEI`, `VValue`, `RValue`, `DeepAbrasion`, `HeatResistance`, `AntiSlip`, `GlazedGranite`, `BoxM2`, `PalletM2`, `DisplayOrder`, `CreatedAt`, `UpdatedAt`. `CreatedAt`/`UpdatedAt` — Category/Collection'da yoktu (ADR-004 gereği eklenmedi), ancak Madde 18.1'in kendi tablosu Product için özel olarak `CreatedDate`/`ModifiedDate`'i "Otomatik" zorunluluk olarak listeliyor; bu doküman-gerekçeli bir istisna. Çevrilebilir alanlar (Name/ShortDescription/LongDescription/SeoUrl/MetaTitle/MetaDescription) yine tamamen `Translation`'da (`EntityType.Product` — zaten Task 3.1B'den beri enum'da mevcuttu, ilk kez bu task'ta gerçek kullanıma girdi).

**Application:** `Application/Products/` — `IProductRepository`, `ProductService`, `ProductDto`, `ProductRequests`, `ProductOperationResult`, `ProductFields`, `ProductEnumDisplay` (Brand/Status için Türkçe etiket eşlemesi — Domain'e sızmadan Application'da). `ProductService`, mevcut `ICategoryRepository`/`ICollectionRepository`/`ITranslationService`/`IUnitOfWork`'ü **değiştirmeden** tekrar kullanıyor; CategoryId/CollectionId var olma kontrolü için mevcut repository'lerin `GetByIdAsync`'i kullanıldı, yeni bir metot eklenmedi.

**İş kuralları (10/10 doğrulandı):** TR ürün adı zorunlu, TR kısa açıklama zorunlu (Madde 18.1: `ShortDescription` Zorunlu — Category/Collection'da olmayan yeni bir kural), ürün kodu global unique (case-insensitive), var olmayan kategori/koleksiyon reddi, kalınlık > 0, zorunlu string alanların (Size/Unit/Surface/BodyType/Color/ApplicationArea/UsageArea) boş olamaması, upsert + opsiyonel alan silme (Relief boşaltıldığında null'a döner), silme + Translation temizliği.

**Infrastructure:** `ProductConfiguration` (ProductCode unique index; CategoryId/CollectionId FK `Restrict`; Brand/Status `HasConversion<string>()` — EntityType'ın aksine, bu enum'lar polimorfik/çapraz-tablo değil, sadece Product'a özgü olduğu için ADR-012'deki gibi ayrı bir açık `ValueConverter` sınıfı yerine standart `HasConversion<string>()` yeterli görüldü; decimal alanlar (`Thickness`/`PEI`/`BoxM2`/`PalletM2`) `decimal(10,3)`), `ProductRepository` (+ `GetByProductCodeAsync` — unique kod kontrolü için).

**Presentation:** `ProductController` (Index/Create/Edit/Delete — Category/Collection'daki `ToggleActive` deseni **yok**, çünkü Product `IsActive bool` değil 4 değerli `Status` enum kullanıyor; durum değişikliği normal Edit formundan yapılıyor), Kategori/Koleksiyon/Marka/Durum dropdown'ları (Kategori 2 seviyeli ağaç flatten edilerek "— " girintisiyle gösteriliyor).

**RBAC — bilinçli kapsam sınırlaması (kritik değil, kayıt altına alındı):** Doküman Madde 30 (Yetkilendirme tablosu) Ürün Yönetimi için **alan-seviyeli** kısmi yetkiler tanımlıyor: İçerik Editörü = yalnızca "Açıklama/Görsel", SEO Editörü = yalnızca "Meta Alanları" düzenleyebilir (Admin ve Ürün Yöneticisi = Tam). Bu, projede şu ana kadar hiç kullanılmamış bir yetkilendirme granülerliği (action-seviyesi `[Authorize(Roles=...)]` yerine, aynı formda alan-bazlı kısmi yazma izni) gerektirir ve yanlış uygulanırsa (örn. formda gizlenen alanların POST body'sine eklenip sunucu tarafında maskeleme yapılmadan kabul edilmesi) güvenlik açığına dönüşebilir. Bu task'ta **Category/Collection ile birebir aynı, action-seviyeli RBAC deseni** uygulandı: Admin+Ürün Yöneticisi = tam CRUD, İçerik Editörü+SEO Editörü = yalnızca görüntüleme, Admin = tek başına silme. Madde 30'daki alan-seviyeli kısmi yetkiler **uygulanmadı** — Admin ve Ürün Yöneticisi (module'ün asıl birincil kullanıcı rolleri) günden itibaren modülü tam kullanabiliyor; İçerik Editörü/SEO Editörü'nün kısmi düzenleme yetkisi ayrı bir backlog maddesi olarak bırakıldı, kullanıcıya bu raporda açıkça bildiriliyor.

**Migration:** `AddProducts` — yalnızca `Products` tablosu (unique `ProductCode` index + `CategoryId`/`CollectionId` FK `Restrict`); `InitialIdentity`/`AddTranslationInfrastructure`/`AddCategories`/`AddCollections`'a dokunulmadı.

**Doğrulama:** Build 0/0; migration içeriği yalnızca beklenen `Products` şemasını içeriyor; 20/20 iş kuralı senaryosu geçici doğrulama koduyla test edildi (geçerli oluşturma, TR ad/kısa açıklama zorunluluğu, duplicate kod reddi, var olmayan kategori/koleksiyon reddi, kalınlık≤0 reddi, boş zorunlu alan reddi, upsert+opsiyonel-alan-silme, Brand/Status güncelleme, silme+Translation temizliği); sqlcmd ile şema + tam temizlik (Products/Categories/Collections/Translations tümü 0 satır, Identity tabloları etkilenmedi — 2 kullanıcı/4 rol aynı) doğrulandı; anonim erişim engeli (`/Product`, `/Product/Create`, `/Product/Delete/1`) curl ile Login'e yönlendirme olarak doğrulandı. Rol-bazlı canlı çoklu-kullanıcı testi, önceki modüllerle aynı credential kısıtı nedeniyle yine yapılamadı.

**Sıradaki iş:** Faz 1 backlog'undaki bir sonraki modül — Ürün Görselleri (#8), Katalog/Doküman Yönetimi (#9), Referans Proje Yönetimi (#10) veya SEO veri sözleşmesi (#4) — henüz başlanmadı, onay bekliyor.

*(Önceki durum korunuyor: Task 4.2 — Koleksiyon Yönetimi — TAMAMLANDI 19.07.2026, bkz. aşağıdaki detay.)*

## Önceki Task Detayı — Task 4.2 — Koleksiyon Yönetimi (uçtan uca) — TAMAMLANDI (19.07.2026).

**Kritik ilişki kararı (Collection ↔ Category) — doküman analiziyle çözüldü, onay istenmedi:** Task 4.1A'da tespit edilen olası çelişki (Madde 16.4'ün "Marka > Kategori > Alt Kategori > Koleksiyon > Ürün" ifadesi) yeniden incelendi. Dört bağımsız doküman kanıtı **Collection'ın Category'ye FK ile bağlı olmadığını** gösteriyor: (1) Madde 20 (Koleksiyon Yönetimi) Category'yi hiç anmıyor; (2) Madde 36.1 (Veri Modeli, Ana Tablolar) `Collections` satırının ilişkilerini yalnızca "Products, Documents" olarak listeliyor — Categories yok; (3) Madde 27.2 (SEO URL) koleksiyon URL'i düz ve bağımsız (`/koleksiyonlar/{seo-url}`), kategori segmenti içermiyor; (4) Madde 18.1 (Ürün Veri Modeli) `Category` ve `SeriesName`/`CollectionName`'i bağımsız kardeş alanlar olarak modelliyor. Madde 16.4'ün ifadesi doküman **Bölüm 16'da** ("Tüm Modüller — Detaylı Gereksinimler", public site modülleri) geçiyor — admin panel şeması değil, kavramsal/gezinme hiyerarşisi. **Sonuç: `Collection` entity'sinde `CategoryId` yok.**

**Domain:** `Collection` (`Id`, `ImagePath`, `DisplayOrder`, `IsActive`) — Category'den daha basit, FK/hiyerarşi yok.

**Application:** `CollectionService`, `ICollectionRepository`, `CollectionDto`/`CollectionRequests`/`CollectionOperationResult`/`CollectionFields` — Category'nin deseniyle birebir ama parent mantığı yok. Mevcut `ITranslationService`/`IUnitOfWork` **değiştirilmeden tekrar kullanıldı** — hiçbir yeni genel-amaçlı metot eklenmedi.

**İş kuralı farkı (Category'ye göre):** Duplicate TR isim kontrolü Category'de "aynı parent altında" iken, Collection'da **global** (Category ilişkisi olmadığı için) — doküman "272 **benzersiz** seri adı" ifadesiyle destekleniyor.

**Presentation:** `CollectionController` (Index/Create/Edit/ToggleActive/Delete, Category dropdown'u yok), düz liste (tree değil). RBAC: Category ile birebir aynı matris.

**Migration:** `AddCollections` — yalnızca `Collections` tablosu, FK yok; Identity/Languages/Translations/Categories'e dokunulmadı.

**Doğrulama:** Build 0/0; 6/6 iş kuralı senaryosu geçti (oluşturma, TR zorunluluğu, global duplicate reddi, upsert+opsiyonel-alan-silme, toggle, silme+Translation temizliği); sqlcmd ile şema+temizlik+diğer tabloların etkilenmediği doğrulandı; anonim erişim engeli doğrulandı. Rol-bazlı canlı test Task 4.1'deki aynı credential kısıtı nedeniyle yapılamadı.

**Sıradaki iş:** Faz 1 backlog'undaki bir sonraki modül (Ürün Yönetimi veya SEO veri sözleşmesi) — henüz başlanmadı, onay bekliyor.

*(Önceki durum korunuyor: Task 4.1 — Kategori Yönetimi — TAMAMLANDI 18-19.07.2026, bkz. aşağıdaki detay.)*

## Önceki Task Detayı — Task 4.1 — Kategori Yönetimi (uçtan uca) — TAMAMLANDI (18-19.07.2026).

Bu task'tan itibaren çalışma düzeni kalıcı olarak değişti: plan + implementasyon + migration + database update + test/doğrulama + dokümantasyon **artık tek task'ta birlikte** yürütülüyor (önceki Task 3.1A/3.1B ayrımı gibi ayrı plan/implementasyon turları yok). Yalnızca dokümanla/ADR'yle çelişen veya katman bağımlılıklarını değiştirecek kritik kararlarda durup onay isteniyor; küçük/geri dönüşü kolay detaylar onay beklemeden uygulanıyor.

**Domain'in ilk gerçek CRUD entity'si:** `Category` (`Id`, `ParentCategoryId`, `ParentCategory`, `Children`, `ImagePath`, `DisplayOrder`, `IsActive`) — sade POCO, self-referencing (2 seviyeli), çevrilebilir hiçbir native sütun yok (Name/Description/SeoUrl/MetaTitle/MetaDescription tamamen merkezi `Translation` tablosunda, `EntityType.Category` ile).

**Application katmanının ilk gerçek kullanımı:** `CategoryService` (interface'siz, somut sınıf), `ICategoryRepository`/`ITranslationService` (Application'da interface, Infrastructure'da implementasyon — Application EF Core'a hiç referans vermiyor), `IUnitOfWork` (tek `SaveChangesAsync` ile Category+Translation atomicity'si için). `CategoryTranslationInput`/`CategoryDto`/`CategoryOperationResult` ile DTO/sonuç modeli kuruldu; generic repository/CQRS/MediatR/interceptor kullanılmadı.

**İş kuralları (10/10 doğrulandı):** TR adı zorunlu, parent mevcut ve ana kategori olmalı, self-parent yasak, 2 seviyeden derin yapı yasak (parent'ın kendi parent'ı olamaz + çocuğu olan kategori parent seçilemez), aynı parent altında aynı TR adı yasak (edit kendi kaydını hariç tutar), alt kategorisi olan kategori silinemez, silme = hard delete + aynı transaction'da Translation temizliği, `IsActive` silmeden ayrı (`ToggleActive`).

**Presentation:** `CategoryController` (Index/Create/Edit/ToggleActive/Delete), ağaç görünümlü liste, Create/Edit formunda tüm aktif diller için çeviri alanları. RBAC: Admin+Ürün Yöneticisi+İçerik Editörü+SEO Editörü görüntüleme, Admin+Ürün Yöneticisi düzenleme, yalnızca Admin silme — doğrudan `[Authorize(Roles=...)]`, policy/claim yok (mevcut projeyle tutarlı).

**Migration:** `AddCategories` — yalnızca `Categories` tablosu, self-referencing FK `Restrict`; `InitialIdentity`/`AddTranslationInfrastructure`'a dokunulmadı.

**Doğrulama:** Build 0/0; migration içeriği yalnızca beklenen `Categories` şemasını içeriyor; geçici `--verify-categories` bayrağıyla CategoryService'in tüm iş kuralları uçtan uca test edildi (10/10 geçti) ve test verisi tamamen temizlendi (sqlcmd ile 0 kalıntı doğrulandı); anonim erişim engeli (`/Category`, `/Category/Delete`) curl ile doğrulandı. **Rol-bazlı çoklu-kullanıcı canlı testi yapılmadı** — gerçek kullanıcı parolaları (User Secrets) bu oturumda paylaşılmadı; RBAC'ın kendisi Task 2.1/2.2'de kanıtlanmış aynı mekanizma olduğu için kod incelemesiyle doğrulandı, canlı çok-rollü test için ayrıca oturum açılması önerilir.

**Küçük, onay istenmeden alınan 2 ek karar (raporda gerekçelendirildi):** `IUnitOfWork` — Application'ın EF Core'suz kalması + tek `SaveChangesAsync` kısıtlarını birlikte sağlamak için zorunlu; `ITranslationService.DeleteTranslationFieldAsync` — boşaltılan opsiyonel çeviri alanının silinmesi kuralı için gerekli.

**Bilinçli olarak yapılmayanlar:** Gerçek dosya yükleme/storage abstraction (ADR-006, ayrı task), arama/filtreleme/sayfalama/toplu işlem/Excel aktarımı, ayrı Details ekranı, drag-drop sıralama, otomatik test projesi, ~~Product/Collection ilişkisi~~ (Collection Task 4.2'de eklendi, Category'ye FK ile bağlı olmadığı netleşti; Product ilişkisi hâlâ yok).

*(Önceki durum korunuyor: Task 3.1B — Translation & Language Infrastructure Implementation — TAMAMLANDI 19.07.2026.)*

## Önceki Task Detayı — Task 3.1B — Translation & Language Infrastructure Implementation — TAMAMLANDI (19.07.2026).

Task 3.1A'da (ADR-012) kesinleşen şema kararlarının koda geçirilmesi. Task 3.1A'da reddedilen/değiştirilen üç karar implementasyon öncesi son kez yeniden değerlendirildi ve uygulandı: `Language.Code` normalizasyonu entity'den çıkarıldı (Application katmanına bırakıldı — Identity'nin `ILookupNormalizer` deseniyle tutarlı), `Language.Translations` navigation collection eklendi (`WithMany(l => l.Translations)`), `Translation.EntityId` tipi `int` olarak sabitlendi (ileride tüm Domain entity'leri için emsal). Audit alanları (CreatedAt/UpdatedAt) ADR-004 ile uyumlu şekilde eklenmedi.

**Oluşturulan dosyalar:**
- `Domain/Enums/EntityType.cs` — 9 üye (Product, Category, Collection, Blog, News, Page, Banner, ReferenceProject, Dealer); Madde 17.2'nin 15 modülünden yalnızca doğrudan izlenebilenler, belirsiz/dayanaksız üyeler (Form, Seo, Menu, Button) eklenmedi.
- `Domain/Entities/Language.cs`, `Domain/Entities/Translation.cs` — sade POCO, private setter, constructor guard (yalnızca gerçek invariant'lar: Code/Name/FieldName boş-whitespace olamaz; normalizasyon/formatlama Domain'de yok).
- `Infrastructure/Persistence/Conversions/EntityTypeMapping.cs` (tek doğruluk kaynağı statik dizi) + `EntityTypeConverter.cs` (`ValueConverter<EntityType,string>`) — `enum.ToString()`'e bağımlı değil, bilinmeyen DB değerinde `InvalidOperationException`.
- `Infrastructure/Persistence/Configurations/LanguageConfiguration.cs`, `TranslationConfiguration.cs` — Fluent API, Data Annotation yok.
- `Infrastructure/Persistence/LanguageSeeder.cs` — tüm ortamlarda (production dahil) çalışır, idempotent, credential gerektirmez (dil verisi sır değil).

**Değiştirilen dosyalar:** `AppDbContext.cs` (ilk `OnModelCreating` override + 2 `DbSet`), `Program.cs` (`SeedLanguagesAsync()` çağrısı).

**Migration:** `AddTranslationInfrastructure` — `Languages` + `Translations` tabloları tek migration'da; `InitialIdentity`'ye dokunulmadı.

**Doğrulama (11/11 geçti):** Tablo şeması, 7 dil doğru sırada (TR→RU, DisplayOrder 1-7) seed edildi, 3 ayrı `dotnet run` ile 0 duplicate (idempotency), `Language.Code` unique index (manuel SQL ile duplicate reddi doğrulandı), `Translation` composite unique index (duplicate reddi doğrulandı), geçersiz `LanguageId` FK tarafından reddedildi, `ON DELETE RESTRICT` çalıştı (RU dili silme denemesi reddedildi, RU **hiç silinmedi**), converter round-trip doğrulandı (`PRODUCT` → doğru enum, `UNKNOWN_XYZ` → beklenen `InvalidOperationException`, sessiz fallback yok), build 0 Warning/0 Error.

**Geçici test kodu/verisi:** Doğrulama için `Program.cs`'e eklenen geçici `--verify-translations` bloğu ve manuel SQL ile eklenen 2 test satırı, doğrulama tamamlandıktan hemen sonra tamamen kaldırıldı/silindi — kalıcı kodda hiçbir iz yok.

**Bilinçli olarak yapılmayanlar (Task 3.1B zamanındaki durum):** ~~`ITranslationCleanupService`/orphan-cleanup abstraction'ı~~ — **Task 4.1'de tasarlandı ve uygulandı** (`ITranslationService.DeleteTranslationsForAsync`, `CategoryService.DeleteAsync` içinde kullanılıyor). Language CRUD/Application servisi, `Code` normalizer servisi, SEO'nun `EntityType` enum'unu paylaşıp paylaşmayacağı kararı (SEO task'ında) — bunlar hâlâ yapılmadı. ~~Herhangi bir Product/Category/vb. entity~~ — **`Category` Task 4.1'de eklendi**, Product hâlâ yok.

*(Önceki durum korunuyor: Task 3.1A — Translation Şema Kararları — TAMAMLANDI 19.07.2026, bkz. aşağıdaki detay.)*

## Önceki Task Detayı — Task 3.1A — Translation (Çoklu Dil) Şema Kararları ve Mimari Analizi — TAMAMLANDI (19.07.2026).

Yalnızca analiz ve mimari karar — **hiçbir entity, DbContext değişikliği, EF configuration, migration veya seeder oluşturulmadı.** ADR-007'nin ilke olarak onayladığı merkezi Translations yaklaşımının bıraktığı 6 şema detayı + ayrı bir `Language` entity'si kesinleştirildi ve **ADR-012** olarak ARCHITECTURE_DECISIONS.md'ye eklendi (ADR-007 değiştirilmedi/geçersiz kılınmadı, yalnızca detaylandırıldı).

**Kesinleşen şema kararları (özet — tam gerekçeler ARCHITECTURE_DECISIONS.md ADR-012'de):**
- `FieldName`: düz `nvarchar`, DB constraint yok, kod tarafında const string sabitler.
- `Value`: tek `nvarchar(max)`, kısa/uzun tüm alanlar aynı sütunda.
- Satır yapısı: her (EntityType, EntityId, LanguageId, FieldName) ayrı satır — JSON/tek-satır model yok.
- `EntityType`: C# enum (Domain'de), Infrastructure'da açık bir `ValueConverter<EntityType,string>` ile string'e çevrilir — varsayılan `HasConversion<string>()`/`enum.ToString()` davranışına bağımlı kalınmaz, eksik mapping'de exception fırlatılır (sessiz fallback yok).
- Unique index (Translation): `(EntityType, EntityId, LanguageId, FieldName)`.
- Yetim çeviri kaydı temizliği: DB trigger/background job yok; entity silme işlemiyle aynı transaction'da Application katmanı servisi (`ITranslationService.DeleteTranslationsForAsync`) yönetir.
- `Language` entity'si ayrıca oluşturulacak (`Id`, `Code`, `Name`, `IsActive`, `DisplayOrder`); `Translation.LanguageId` → `Language.Id` **klasik (surrogate) FK** — doğal anahtar (`Language.Code`) üzerinden FK **tercih edilmedi** (EF Core idiomu, migration basitliği, maintainability gerekçesiyle Id lehine karar verildi). `Language.Code` ayrı bir unique index ile korunur. `Translation`'da ayrıca `LanguageCode` sütunu tutulmaz. Silme davranışı `ON DELETE RESTRICT` — kullanılan bir dil fiziksel olarak silinemez, yalnızca `IsActive=false` ile pasif yapılır.
- Migration: `Language` + `Translation` Task 3.1B'de **tek migration**da birlikte oluşturulacak (FK bağımlılığı zaten bunu gerektiriyor); dil verisi (TR/EN/DE/FR/ES/AR/RU) migration'a gömülmeyecek, `IdentitySeeder` deseniyle ayrı bir seeder ile eklenecek.

**Bu task kapsamında ayrıca:** Backlog madde #2 ("Kullanıcı/Rol Yönetimi") "RBAC altyapısı tamamlandı, Kullanıcı/Rol CRUD yönetim ekranları ileride yapılacak" ifadesiyle tamamlandı işaretlendi (TASKS.md).

*(Task 3.1B artık tamamlandı — bkz. yukarıdaki "Bulunduğumuz Task" bölümü.)*

*(Önceki durum korunuyor: Task 2.2 — Role-Based Authorization — TAMAMLANDI 18.07.2026.)*

## Önceki Task Detayı — Task 2.2 — Role-Based Authorization — TAMAMLANDI (18.07.2026).

`HomeController.AdminOnly()` (`[Authorize(Roles = ApplicationRoles.Admin)]`) eklendi; `IdentitySeeder`'a Development-only, idempotent bir `SeedDevelopmentTestUserAsync` metodu eklendi (İçerik Editörü rolünde test kullanıcısı, `Program.cs`'te yalnızca `if (app.Environment.IsDevelopment())` altında çağrılıyor — environment kontrolü Infrastructure'a sızmadı). 11 doğrulama senaryosunun tamamı gerçek admin + test kullanıcı credential'larıyla test edildi ve geçti; Production ortamı simülasyonuyla (yalnızca ortam değişkeni ile, dosya değişikliği olmadan) test kullanıcı seed'inin çağrılmadığı ampirik olarak kanıtlandı (bkz. "Task 2.2" detay bölümü aşağıda).

*(Önceki durum korunuyor: Task 2.1 — Authentication UI — TAMAMLANDI 18.07.2026. Task 1.2C/1.2B/1.2A/1.1 — TAMAMLANDI. Task 0.2 — tüm 10 karar ADR-002 → ADR-011 olarak kayıtlı. Task 1 (Identity Foundation) tamamen kapalı.)*

Karar sırası:
1. Mevcut proje teslimatının kesin teknik sınırı — **Onaylandı (17.07.2026, Seçenek A). Bkz. ARCHITECTURE_DECISIONS.md ADR-002.**
2. ASP.NET Core uygulama tipi ve panelin sunum modeli — **Onaylandı (17.07.2026, Seçenek 1 — server-rendered MVC). Bkz. ARCHITECTURE_DECISIONS.md ADR-003.**
3. EF Core Code First / Database First — **Onaylandı (17.07.2026, Code First — mevcut/hazır veritabanı yok teyit edildi). Bkz. ARCHITECTURE_DECISIONS.md ADR-004.**
4. Yönetim paneli authentication yöntemi — **Onaylandı (17.07.2026, ASP.NET Core Identity + Cookie Authentication). Bkz. ARCHITECTURE_DECISIONS.md ADR-005. 2FA detayları ve şifre sıfırlama akışı bilinçli olarak açık bırakıldı (aşağıya eklendi).**
5. Dosya ve görsel saklama yaklaşımı — **Onaylandı (17.07.2026, Yerel Dosya Sistemi + storage abstraction). Bkz. ARCHITECTURE_DECISIONS.md ADR-006. Dosya isimlendirme standardı ve WebP/AVIF/CDN alt kararı açık bırakıldı.**
6. Çoklu dil veri modeli ve fallback davranışı — **Onaylandı (17.07.2026, veri modeli: merkezi Translations/A1 — Madde 36.1). Bkz. ARCHITECTURE_DECISIONS.md ADR-007. Nihai şema detayları ve yetim kayıt temizleme stratejisi açık bırakıldı; fallback davranışı "Gelecek Faz / Karar Bekleniyor" durumunda.**
7. Showroom ayrı modül mü, Bayi kategorisi mi — **Onaylandı (17.07.2026, tek Dealer entity + Category ayrımı). Bkz. ARCHITECTURE_DECISIONS.md ADR-008. Kategorisiz kayıtlar ve showroom-özel alanların kapsamı açık bırakıldı.**
8. Public site ile gelecekteki entegrasyon sınırı — **Onaylandı (17.07.2026, ek iskelet/DTO/contract yok; katman ayrımı yeterli — YAGNI). Bkz. ARCHITECTURE_DECISIONS.md ADR-009.**
9. SAP entegrasyonunun Faz 1/Faz 2 konumu — **Onaylandı (17.07.2026, tamamen Faz 2, Faz 1'de hiçbir hazırlık yok). Bkz. ARCHITECTURE_DECISIONS.md ADR-010.**
10. Güvenlik gereksinimlerinin teknik karşılıkları — **Onaylandı (17.07.2026, yerleşik Rate Limiting + Serilog/ILogger + MIME/magic-byte/whitelist dosya doğrulama). Bkz. ARCHITECTURE_DECISIONS.md ADR-011.**

**Task 0.2 — TAMAMLANDI (17.07.2026). Tüm 10 karar onaylandı.**

## Tamamlanan Tasklar

- **Task 0.1 — Kavramsal Analiz Çıkarımı** (17.07.2026, ilk sürüm)
  - **Durum: Düzeltildi (17.07.2026, ikinci geçiş).** İlk sürümdeki hatalar aşağıdaki "Düzeltme Günlüğü" bölümünde kayıtlıdır. Bu bölümdeki asıl analiz içeriği geçerli kabul edilmemeli; güncel ve doğrulanmış içerik bu dosyanın "Modül Haritası" ve "Roller" bölümlerindedir.
  - **Durum: Kullanıcı tarafından onaylandı (17.07.2026).**

### Düzeltme Günlüğü (Task 0.1 — İkinci Geçiş)

| # | İlk Sürümdeki Kayıt | Durum | Düzeltme |
|---|---|---|---|
| 1 | "14 modül" (Blog ve Haber Yönetimi tek satırda birleştirilmişti) | **Düzeltildi** | Doküman madde 17.2 tablosu 15 satır içeriyor; Blog Yönetimi ve Haber Yönetimi dokümanda ayrı satırlardır. Doğru sayı: **15 modül**. Aşağıdaki "Modül Haritası" bölümüne bakınız. |
| 2 | İçerik Editörü rolü tanımında "ürün açıklamaları (onay akışı ile)" bir iş akışı/gereksinim gibi sunulmuştu | **Geçersiz Kılındı** | Doküman madde 7.2'de bu ifade sadece rol-yetki kapsamı notu olarak geçiyor; onay akışının adımları, durumları veya onaylayıcısı hiçbir yerde tanımlanmıyor. Bu nedenle bir iş akışı/modül olarak ele alınmayacak, açık karar listesine de eklenmeyecek — dokümanda böyle bir gereksinim yok. |
| 3 | "SEO en sonda, diğer modüllerden bağımsız" sıralama mantığı | **Düzeltildi** | SEO'nun veri sözleşmesi (SeoMeta alan/ilişki deseni — madde 36.1'de EntityType/EntityId ile polimorfik ilişki olarak zaten dokümanda tanımlı) erken tasarlanmalı; sadece SEO **Yönetim modülünün (UI)** geliştirilmesi içerik modüllerinden sonra yapılabilir. Bkz. TASKS.md güncel sıralama. |
| 4 | Güvenlik gereksinimleri tek liste halinde, teknik uygulama detayına girmeden sunulmuştu ama ayrım yapılmamıştı | **Düzeltildi** | Aşağıda "Güvenlik Gereksinimleri" bölümünde doküman-zorunlu olanlar ile teknik uygulama kararı gerektirenler ayrıldı. |
| 5 | Modül/rol/bağımlılık analizi "Dokümanda Açıkça Tanımlananlar" ile "Teknik Çıkarımlar" ayrımı yapılmadan tek blokta sunulmuştu | **Düzeltildi** | Aşağıdaki bölümler üç başlığa ayrıldı: Dokümanda Açıkça Tanımlananlar / Teknik Çıkarımlar / Karar Bekleyen Konular. |

## Bekleyen Tasklar

> Detaylı task kırılımı TASKS.md'de. Faz 1 başlamadan önce her modül için ayrı task olarak onaya sunulacak.

- Task 0.2: Açık teknik kararların (10 madde, bkz. "Bulunduğumuz Task") kullanıcı ile tek tek netleştirilmesi — **devam ediyor**, Karar #1 sunuldu.
- Task 0.2 tamamlandıktan sonra: Faz 1'in ilk uygulama task'ı (solution/katman iskeleti) — henüz başlamadı.

## Oluşturulan Katmanlar

**Task 1.1 (17.07.2026) ile oluşturuldu:**

| Proje | Tip | Referans Verdiği Projeler | Hedef Framework |
|---|---|---|---|
| `Presentation` | ASP.NET Core Empty (MVC servisleri koddan eklendi) | Application, Infrastructure | net10.0 |
| `Application` | Class Library | Domain | net10.0 |
| `Domain` | Class Library | (yok — bağımsız) | net10.0 |
| `Infrastructure` | Class Library | Application, Domain | net10.0 |

Solution: `NGKutahyaSeramik_AdminPanel.sln` (`src/` altında dört proje). Döngüsel bağımlılık yok.

### Task 1.1 — Uygulama Kayıtları (Doğrulama Sonuçları)

- `dotnet restore` — **Başarılı.**
- `dotnet build` (tüm solution) — **Başarılı, 0 Warning / 0 Error.**
- `dotnet run` (Presentation, Development ortamı) — **Başarılı**, unhandled exception yok. Log: `Application started`, `Now listening on: http://localhost:5167`.
- `GET /` isteği — **404** döndü (beklenen davranış — hiçbir Controller/action henüz yok).
- İlk denemede `wwwroot` klasörü yoktu → `UseStaticFiles()` bir **uyarı** (WRN, hata değil) üretti: *"WebRootPath was not found... Static files may be unavailable."* Boş bir `wwwroot/` klasörü eklenerek bu uyarı giderildi (ikinci çalıştırmada log temizdi).
- Derleme sırasında Application/Infrastructure class library'lerinde `IServiceCollection`/`IConfiguration` tipleri bulunamadı (plain class library'ler ASP.NET Core shared framework'ünü örtük almıyor) → `Microsoft.Extensions.DependencyInjection.Abstractions` (Application + Infrastructure) ve `Microsoft.Extensions.Configuration.Abstractions` (yalnızca Infrastructure) paketleri eklendi; bu, derleme hatası nedeniyle gerçekten gerekli olduğu doğrulanan tek ek paket kararıdır.
- `_ViewImports.cshtml` içindeki `@using Presentation` satırı derleme hatası verdi (projede henüz bu ad altında hiçbir namespace/tip yok) → satır kaldırıldı, yalnızca `@addTagHelper` bırakıldı.

### Task 1.2A (17.07.2026) ile Eklenen Identity/DbContext Altyapısı

| Dosya | Sorumluluk |
|---|---|
| `Infrastructure/Identity/ApplicationUser.cs` | `IdentityUser`'dan türeyen kullanıcı sınıfı, ek property yok. |
| `Infrastructure/Identity/ApplicationRoles.cs` | 4 rol için sabit string (Admin, İçerik Editörü, SEO Editörü, Ürün Yöneticisi). |
| `Infrastructure/Persistence/AppDbContext.cs` | `IdentityDbContext<ApplicationUser>`'dan türeyen DbContext, domain `DbSet` yok, `OnModelCreating` override edilmedi. |
| `Infrastructure/DependencyInjection.cs` | `AddDbContext<AppDbContext>` (UseSqlServer, connection string guard clause) + `AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders()`. Identity varsayılan parola/lockout ayarları **değiştirilmedi**. |
| `Presentation/Program.cs` | `ConfigureApplicationCookie` (LoginPath, AccessDeniedPath, HttpOnly, SecurePolicy=Always, SameSite=Lax — ExpireTimeSpan/SlidingExpiration'a dokunulmadı), `AddAuthorization()` (bare), `UseAuthentication()`/`UseAuthorization()` (routing sonrası, MapControllerRoute'tan önce). |
| `Presentation/appsettings.json` | `ConnectionStrings:DefaultConnection` boş string (production placeholder, gerçek secret yok). |
| `Presentation/appsettings.Development.json` | `Server=.\SQLEXPRESS;...;Trusted_Connection=True` — bu makinede **gerçekten çalışan** SQL Server Express instance'ı (`net start` ile doğrulandı) referans alınıyor, kimlik bilgisi içermiyor (Windows Trusted Connection). |
| `Infrastructure/Infrastructure.csproj` | `<FrameworkReference Include="Microsoft.AspNetCore.App" />` eklendi (bkz. karşılaşılan hata #2 aşağıda) + EF Core/Identity 9.0.18 paketleri. |
| `Presentation/Presentation.csproj` | `Microsoft.EntityFrameworkCore.Design 9.0.18` eklendi (CLI migration akışı için, Tools paketi eklenmedi). |

**Karşılaşılan hatalar ve çözümleri:**
1. Plain class library projelerinde (Application/Infrastructure) framework paketleri örtük gelmiyor — beklenen ve önceden bilinen durum (Task 1.1'de de yaşanmıştı).
2. `AddIdentity<ApplicationUser, IdentityRole>()` Infrastructure'da derleme hatası verdi (`AddIdentity` bulunamadı) — kök neden: bu metod ASP.NET Core paylaşılan çatısının (`Microsoft.AspNetCore.App`) parçası, düz class library bunu otomatik almıyor. Çözüm: `Infrastructure.csproj`'a `<FrameworkReference Include="Microsoft.AspNetCore.App" />` eklendi (NuGet paketi değil, sürüm numarası yok, standart/resmi çözüm).

**Doğrulama:** `dotnet restore` ✅, `dotnet build` ✅ (0 Warning/0 Error), `dotnet run` ✅ (unhandled exception yok, DB'ye erişen hiçbir işlem gözlenmedi), `GET /` → 404 (beklenen).

### Task 1.2B (18.07.2026) ile Eklenen Migration/Seed Altyapısı

| Dosya/İşlem | Sorumluluk |
|---|---|
| `Infrastructure/Persistence/Migrations/20260717210920_InitialIdentity.cs` (+ Designer + Snapshot) | İlk EF Core migration — yalnızca 7 Identity tablosu (AspNetRoles, AspNetUsers, AspNetRoleClaims, AspNetUserClaims, AspNetUserLogins, AspNetUserRoles, AspNetUserTokens), domain tablosu yok. |
| `Infrastructure/Identity/IdentitySeeder.cs` | Extension method (`SeedIdentityDataAsync`) — rol seed (koşulsuz, idempotent, `ApplicationRoles` sabitlerini kullanır) + admin seed (`SeedAdmin:Email`/`Password` User Secrets'tan okunur, eksikse WARNING loglanır ve atlanır). |
| `Presentation/Program.cs` | `app.Build()` sonrası, `app.Run()` öncesi, `IServiceScope` içinde `await scope.ServiceProvider.SeedIdentityDataAsync()` çağrısı; hata durumunda `ILogger` ile loglanıp yeniden fırlatılıyor (yutulmuyor). |
| `Presentation/Presentation.csproj` | `<UserSecretsId>b6cc62d6-5a57-4e25-a3a3-fa9bbe00a79c</UserSecretsId>` eklendi (`dotnet user-secrets init`) — **hiçbir gerçek secret değeri girilmedi**, store boş. |

**Çalıştırılan komutlar:**
1. `dotnet ef dbcontext info` (design-time doğrulama, mevcut global `dotnet-ef 10.0.9` ile) — ✅ başarılı, hiçbir sürüm uyumsuzluğu çıkmadı → repo-yerel `dotnet-ef 9.0.18` kurulumu **gerekmedi**.
2. `dotnet ef migrations add InitialIdentity --output-dir Persistence/Migrations` — ✅.
3. `dotnet ef database update` — ✅, `NGKutahyaSeramikAdminPanel` veritabanı `.\SQLEXPRESS` üzerinde oluşturuldu.
4. `dotnet user-secrets init` (Presentation) — ✅, `UserSecretsId` eklendi, **değer girilmedi**.
5. `dotnet build` — ✅ 0/0.
6. `dotnet run` (Development) — ✅: 4 rol INSERT edildi, `WRN: SeedAdmin:Email / SeedAdmin:Password tanımlı değil — admin kullanıcı seed'i atlandı.` logu görüldü, uygulama çökmeden ayakta kaldı.
7. `sqlcmd` doğrulaması: `AspNetRoles` → 4 satır (Admin, İçerik Editörü, SEO Editörü, Ürün Yöneticisi — `LEN()` ile 14/11/15 karakter uzunlukları doğrulandı, terminaldeki bozuk görünüm yalnızca konsol kod sayfası sorunuydu, veri doğru); `AspNetUsers` → **0 satır** (admin kullanıcı yok, beklenen).

**Migration/database update sırası:** Yalnızca CLI ile manuel yapıldı; `Database.MigrateAsync()` kodun hiçbir yerinde kullanılmadı (ADR-004'teki "üretim migration yöntemi" açık kararı kasıtlı olarak kapatılmadı).

### Task 1.2C (18.07.2026) — İlk Admin Kullanıcısının Seed Edilmesi

- Kullanıcı tarafından sağlanan gerçek development admin e-postası/parolası, `dotnet user-secrets set "SeedAdmin:Email" ...` ve `dotnet user-secrets set "SeedAdmin:Password" ...` ile Presentation projesinin User Secrets deposuna kaydedildi. Hiçbir dosyaya (appsettings.json, appsettings.Development.json, kaynak kod) yazılmadı — grep ile doğrulandı, kodda yalnızca config anahtar adları (`"SeedAdmin:Email"`/`"SeedAdmin:Password"`) geçiyor, gerçek değerler geçmiyor.
- `dotnet build` → 0 Warning / 0 Error.
- **1. çalıştırma (Development):** Log'da `INSERT INTO AspNetUsers` ve `INSERT INTO AspNetUserRoles` görüldü (parametre değerleri EF Core tarafından `?` ile maskeli, hiçbir credential loglanmadı). SQL doğrulaması: `Email=admin@localhost`, `UserName=admin@localhost`, `EmailConfirmed=0` (framework varsayımı, değiştirilmedi), `RoleName=Admin`. `UserCount=1`.
- **2. çalıştırma (Development):** Log'da yalnızca `SELECT` sorguları var, **hiçbir `INSERT` yok** — idempotency kod seviyesinde doğrulandı. SQL doğrulaması: `UserCount=1` (değişmedi), `UserRoleCount=1` (değişmedi), `CHECKSUM(PasswordHash)`/`CHECKSUM(SecurityStamp)`/`CHECKSUM(ConcurrencyStamp)` **birebir aynı** (231424884 / -1674612938 / -960776378 — her iki çalıştırmada da) → parola/security stamp hiç dokunulmadı.
- **PasswordHash, SecurityStamp, ConcurrencyStamp veya User Secrets değerleri hiçbir yerde (log, rapor, bu dosya) açık gösterilmedi** — yalnızca checksum karşılaştırması kullanıldı.

**7 doğrulama noktası — sonuç:**
| # | Kontrol | Sonuç |
|---|---|---|
| 1 | Admin kullanıcı AspNetUsers'da oluştu | ✅ |
| 2 | Kullanıcı Admin rolüne bağlandı | ✅ |
| 3 | EmailConfirmed framework varsayımında kaldı | ✅ (0/false) |
| 4 | İkinci çalıştırmada duplicate kullanıcı yok | ✅ (UserCount=1) |
| 5 | Duplicate rol ilişkisi yok | ✅ (UserRoleCount=1) |
| 6 | Mevcut kullanıcının parolası overwrite edilmedi | ✅ (checksum'lar birebir aynı) |
| 7 | Build 0 Warning / 0 Error | ✅ |

### Task 2.1 (18.07.2026) — Authentication UI

**Oluşturulan dosyalar:** `Presentation/Controllers/AccountController.cs` (Login GET/POST, Logout POST, AccessDenied GET), `Presentation/Controllers/HomeController.cs` (`[Authorize]`, Index), `Presentation/Models/Account/LoginViewModel.cs` (Email/Password/ReturnUrl), `Presentation/Views/Shared/_Layout.cshtml` (minimal — başlık + `@RenderBody()` + authenticated ise POST Logout formu), `Presentation/Views/Account/Login.cshtml`, `Presentation/Views/Account/AccessDenied.cshtml`, `Presentation/Views/Home/Index.cshtml`.

**Değişen dosyalar:** `Presentation/Views/_ViewStart.cshtml` (Layout ataması eklendi), **`Presentation/Program.cs`** (bkz. aşağıdaki kritik bulgu — gerçekten zorunlu olduğu için değiştirildi).

**Kritik bulgu ve düzeltme:** İmplementasyon sırasında `[Authorize]` denendiğinde `System.InvalidOperationException: ... a middleware was not found that supports authorization` hatası alındı. Kök neden: Task 1.2A'da `ConfigureApplicationCookie`/`AddAuthorization()` **servisleri** kaydedilmişti ama `app.UseAuthentication()`/`app.UseAuthorization()` **middleware'leri** pipeline'a hiç eklenmemişti — o zamana kadar hiçbir `[Authorize]` action olmadığı için fark edilmemişti. `UseRouting()` sonrası, `MapControllerRoute` öncesine iki satır eklendi. Bu, planın "yalnızca gerçekten zorunlu bir eksik tespit edilirse değiştir" kısıtına tam uyan, zorunlu bir düzeltmeydi.

**Test metodolojisi notu:** İlk ReturnUrl/open-redirect testlerinde yanlışlıkla `ReturnUrl`'i POST body'sine gömdüm; gerçek form bunu `asp-route-returnUrl` ile action URL'sinin **query string**'inde taşıyor. Gerçek tarayıcı davranışını birebir taklit eden (GET ile `?ReturnUrl=...` al → o action URL'ine POST et) yöntemle testler tekrarlanıp doğru şekilde doğrulandı. Ayrıca ilk Logout denemelerinde antiforgery token/cookie senkronizasyon hatası (test script'inin stale bir cookie jar'ı yeniden kullanması) yaşandı; temiz/tek seferlik bir akışla (login → sayfa çek → hemen logout) doğru şekilde doğrulandı. Her iki durum da **uygulama kodunda değil, test script'imde** kaynaklıydı; ikisi de düzeltilip doğru sonuçla tekrar test edildi.

**11 Doğrulama Senaryosu — Sonuç:**

| # | Senaryo | Sonuç |
|---|---|---|
| 1 | Anonim `/Home/Index` → `/Account/Login?ReturnUrl=%2FHome%2FIndex` | ✅ |
| 2 | Doğru admin bilgileriyle başarılı giriş (Home/Index'te e-posta doğru gösterildi) | ✅ |
| 3 | Yanlış parola → genel hata mesajı ("Geçersiz e-posta veya parola.") | ✅ |
| 4 | Kayıtlı olmayan e-posta → **aynı** genel hata mesajı | ✅ |
| 5 | Harici ReturnUrl (`https://evil.com`, gerçek form mekanizmasıyla) → Home/Index'e düştü, dış siteye yönlendirilmedi | ✅ |
| 6 | Login olmuş kullanıcı `Login` GET'e gider → Home'a otomatik yönlendirme | ✅ |
| 7 | Logout POST sonrası cookie geçersiz (tekrar Home/Index → Login'e yönlendirme) | ✅ |
| 8 | Logout'a GET isteği → 405 Method Not Allowed | ✅ |
| 9 | AccessDenied ekranı doğrudan URL ile render edildi | ✅ |
| 10 | Birden fazla giriş denemesi sonrası kullanıcı/rol verisi değişmedi (UserCount=1, RoleCount=4, UserRoleCount=1) | ✅ |
| 11 | `dotnet build` → 0 Warning / 0 Error | ✅ |

Gerçek admin parolası, PasswordHash, User Secrets değerleri veya hassas cookie içeriği hiçbir log/rapor/dosyada açık gösterilmedi.

### Task 2.2 (18.07.2026) — Role-Based Authorization

**Oluşturulan/değiştirilen dosyalar:** `Presentation/Controllers/HomeController.cs` (yeni `AdminOnly()` action, `[Authorize(Roles = ApplicationRoles.Admin)]`), `Presentation/Views/Home/AdminOnly.cshtml` (yeni, minimal, iş mantığı yok), `Infrastructure/Identity/IdentitySeeder.cs` (yeni `SeedDevelopmentTestUserAsync` + `SeedTestUserAsync` — admin seed deseniyle birebir, `ApplicationRoles.ContentEditor` kullanılıyor, mevcut admin seed davranışı değişmedi), `Presentation/Program.cs` (yalnızca `if (app.Environment.IsDevelopment()) { await scope.ServiceProvider.SeedDevelopmentTestUserAsync(); }` eklendi). `ApplicationRoles.cs`'e dokunulmadı, named policy oluşturulmadı, Domain/Application değişmedi, yeni paket eklenmedi.

**User Secrets (Presentation):** `SeedTestUser:Email`, `SeedTestUser:Password` kaydedildi — appsettings.json/appsettings.Development.json/kaynak koda hiçbir değer yazılmadı.

**11 Doğrulama Senaryosu — Sonuç:**

| # | Senaryo | Sonuç |
|---|---|---|
| 1 | Anonim `/Home/Index` → Login'e `ReturnUrl` ile yönlendirme | ✅ |
| 2 | Anonim `/Home/AdminOnly` → Login'e `ReturnUrl` ile yönlendirme | ✅ |
| 3 | Admin kullanıcı → `/Home/AdminOnly` → 200 OK | ✅ |
| 4 | İçerik Editörü (test kullanıcı) → `/Home/Index` → 200 OK | ✅ |
| 5 | İçerik Editörü → `/Home/AdminOnly` → **organik** `AccessDenied` yönlendirmesi (302) | ✅ |
| 6 | AccessDenied ekranı organik yönlendirme sonrası render edildi | ✅ |
| 7 | Rol/kullanıcı sayıları doğru (`UserCount=2`, `RoleCount=4`, `UserRoleCount=2`; `EmailConfirmed=0` her ikisinde de) | ✅ |
| 8 | İkinci çalıştırmada duplicate kullanıcı/rol ilişkisi yok (0 `INSERT`) | ✅ |
| 9 | Mevcut kullanıcıların parolaları overwrite edilmedi (checksum'lar iki çalıştırmada da birebir aynı) | ✅ |
| 10 | Production ortamı simülasyonunda (`ASPNETCORE_ENVIRONMENT=Production`, `--no-launch-profile`, yalnızca env var ile geçici connection string) test kullanıcı seed'i **çağrılmadı** (log'da hiçbir iz yok) | ✅ |
| 11 | `dotnet build` → 0 Warning / 0 Error | ✅ |

**Önemli davranış notu (belgelendi, çözülmedi):** Identity role claim'leri login sırasında authentication cookie'ye eklenir. Veritabanındaki sonraki rol değişikliklerinin mevcut cookie'ye anında yansıması garanti edilmez; güncel rollerin alınması için yeniden authentication (logout+login) gerekir. Bu task'ta bu davranış için security stamp validation interval veya otomatik cookie yenileme eklenmedi — kullanıcının kararıyla kapsam dışı bırakıldı, geliştirme verisi üzerinde manuel rol değiştirme testi de yapılmadı (risk/gereksizlik gerekçesiyle).

Gerçek test kullanıcı parolası, PasswordHash, SecurityStamp, ConcurrencyStamp, User Secrets değerleri hiçbir log/rapor/dosyada açık gösterilmedi.

## Oluşturulan Entityler

**Domain'deki ilk gerçek entity'ler — Task 3.1B (19.07.2026) ile eklendi:**

- **`Language`** (`Domain/Entities/Language.cs`) — `Id`(int), `Code`(nvarchar(10), unique), `Name`(nvarchar(100)), `IsActive`(bool, default true), `DisplayOrder`(int); sade POCO, private setter, constructor guard (Code/Name boş-whitespace olamaz), `Translations` navigation collection.
- **`Translation`** (`Domain/Entities/Translation.cs`) — `Id`(int), `EntityType`(enum), `EntityId`(int), `LanguageId`(int, FK → `Language.Id`, `ON DELETE RESTRICT`), `FieldName`(nvarchar(100)), `Value`(nvarchar(max), NOT NULL ama boş string kabul edilir); composite unique index `(EntityType,EntityId,LanguageId,FieldName)`.
- **`EntityType`** (`Domain/Enums/EntityType.cs`) — enum, 9 üye (Product, Category, Collection, Blog, News, Page, Banner, ReferenceProject, Dealer); Infrastructure'da açık `ValueConverter` ile string'e çevrilir (`enum.ToString()` kullanılmaz).

**Domain'in ilk gerçek CRUD entity'si — Task 4.1 (18-19.07.2026) ile eklendi:**

- **`Category`** (`Domain/Entities/Category.cs`) — `Id`(int), `ParentCategoryId`(int?, self-referencing FK → `Category.Id`, `ON DELETE RESTRICT`), `ParentCategory`/`Children` navigation, `ImagePath`(nvarchar(500), nullable, düz metin — dosya yükleme yok), `DisplayOrder`(int, default 0), `IsActive`(bool, default true). Çevrilebilir hiçbir native sütun yok — Name/Description/SeoUrl/MetaTitle/MetaDescription tamamen `Translation` tablosunda (`EntityType.Category`). `UpdateDetails`/`Activate`/`Deactivate` metotları var (Language'dan farklı olarak — Category'nin kendi CRUD'ı bu task'ta yapıldığı için mutator'lara gerçek ihtiyaç var).

**Task 4.2 (19.07.2026) ile eklenen ikinci CRUD entity'si:**

- **`Collection`** (`Domain/Entities/Collection.cs`) — `Id`(int), `ImagePath`(nvarchar(500), nullable), `DisplayOrder`(int, default 0), `IsActive`(bool, default true). Category'den daha basit — **`CategoryId` yok**, hiçbir FK ilişkisi yok (dört bağımsız doküman kanıtıyla Category'ye bağlı olmadığı netleşti — Madde 20, 36.1, 27.2, 18.1; ayrıntı yukarıdaki "Bulunduğumuz Task" bölümünde). Çevrilebilir alanlar (Name/Description/SeoUrl/MetaTitle/MetaDescription) `Translation`'da (`EntityType.Collection`).

**Task 5 (19.07.2026) ile eklenen üçüncü CRUD entity'si (projenin ilk gerçek `Category`+`Collection` ilişkili entity'si):**

- **`Product`** (`Domain/Entities/Product.cs`) — `Id`, `ProductCode`(unique), `CategoryId`/`Category` (FK→Category, zorunlu), `CollectionId`/`Collection` (FK→Collection, zorunlu — `SeriesName`'in karşılığı, ayrıntı yukarıda), `Brand`(enum: NgSeramik/NgStone/NgSlim/NgPerforma), `Status`(enum: Active/Inactive/InProgress/Cancelled — Category/Collection'ın `IsActive bool`'undan farklı), `Size`, `Unit`, `Surface`, `Relief`, `SpecialSurface`, `FaceCount`, `Thickness`, `BodyType`, `Color`, `ColorMaterial`, `ApplicationArea`, `UsageArea`, `Finish`, `PEI`, `VValue`, `RValue`, `DeepAbrasion`, `HeatResistance`, `AntiSlip`, `GlazedGranite`, `BoxM2`, `PalletM2`, `DisplayOrder`, `CreatedAt`, `UpdatedAt` (doküman-gerekçeli istisna — Category/Collection'da audit alanı yok). Çevrilebilir alanlar (Name/ShortDescription/LongDescription/SeoUrl/MetaTitle/MetaDescription) `Translation`'da (`EntityType.Product`). Doküman/referans proje ilişkileri bu task'ta yok — ayrı backlog maddeleri (#9/#10). Görsel ilişkisi Task 5.1 ile eklendi (aşağıda).

**Task 5.1 (19.07.2026) ile eklenen dördüncü CRUD entity'si (Product'ın ilk çocuk entity'si):**

- **`ProductImage`** (`Domain/Entities/ProductImage.cs`) — `Id`, `ProductId`/`Product` (FK, `Cascade`), `ImageType`(enum: Render/Face/Lifestyle/Texture/Detail — Madde 18.2), `FilePath`(nvarchar(500), web-relative, örn. `/uploads/products/55018167RP/face/<guid>.jpg`), `IsPrimary`(bool), `DisplayOrder`(int). Çevrilebilir alan yok (görseller Translation'a taşınmadı — doküman gerektirmiyor). DB'de filtered unique index (`ProductId` üzerinde, `WHERE IsPrimary=1`) ile üründe en fazla bir ana görsel garantisi.

**Task 6 (19.07.2026) ile eklenen entity'ler (Backlog #9 — Katalog/Doküman Yönetimi):**

- **`Document`** (`Domain/Entities/Document.cs`) — `Id`, `DocumentType`(enum: Catalog/TechnicalSheet/Certificate/Report — Madde 24'ün kapalı listesi), `Title`(nvarchar(300), **native sütun, Translation değil** — ayrıntı yukarıda "Kritik bulgu 1"), `LanguageId`/`Language` (FK→`Language`, `Restrict`), `FilePath`, `OriginalFileName`, `FileExtension`, `ContentType`, `FileSize`(long), `DisplayOrder`, `IsActive`(bool — Category/Collection deseniyle aynı, Product'ın 4-değerli enum'undan farklı). Audit alanı yok (Madde 24 istemiyor). `Product`/`Collection`'a doğrudan FK **yok** — ilişki M2M.
- **`ProductDocument`** (`Domain/Entities/ProductDocument.cs`) — junction entity, `ProductId`+`DocumentId`, her ikisi de `Cascade`, composite unique index.
- **`CollectionDocument`** (`Domain/Entities/CollectionDocument.cs`) — junction entity, `CollectionId`+`DocumentId`, her ikisi de `Cascade`, composite unique index.

Task 1.2A ile Infrastructure'a eklenen Identity altyapı sınıfları (bunlar domain entity değildir):

- **`ApplicationUser`** (`Infrastructure/Identity/ApplicationUser.cs`) — `IdentityUser`'dan türeyen, hiçbir ek property içermeyen boş bir türetme.
- **`ApplicationRoles`** (`Infrastructure/Identity/ApplicationRoles.cs`) — 4 rol için sabit string tanımları (Admin, İçerik Editörü, SEO Editörü, Ürün Yöneticisi). Enum değil, seed edilmiş bir veri de değil — sadece string sabitleri.
- **`IdentitySeeder`** (`Infrastructure/Identity/IdentitySeeder.cs`) — Task 1.2B ile eklendi, rol+admin seed mantığını içeren extension method.

**Veritabanında seed edilmiş veri:** 4 rol (`AspNetRoles`, Task 1.2B). 1 admin kullanıcı (`AspNetUsers`, Task 1.2C — `admin@localhost`, Admin rolüne bağlı, idempotency doğrulandı). **7 dil** (`Languages`, Task 3.1B — TR/EN/DE/FR/ES/AR/RU, `DisplayOrder` 1-7, idempotency 3 ayrı çalıştırmayla doğrulandı). `Translations`, `Categories` ve `Collections` tabloları şu an **0 satır** — Task 4.1/4.2'nin doğrulama testlerinde eklenen geçici veriler tamamen temizlendi (sqlcmd ile 0 kalıntı doğrulandı); `CategorySeeder`/`CollectionSeeder` bilinçli olarak **oluşturulmadı** (Madde 19.1'deki 13 kategori ve 272 seri adı, veri aktarımı/Excel import task'ında ele alınacak, `LanguageSeeder` gibi zorunlu sistem verisi sayılmıyor). Şifre/hash değerleri bu dosyada veya herhangi bir raporda gösterilmez.

## Oluşturulan Endpointler

Henüz yok. **Not:** Public site'a ait hiçbir endpoint adı/tasarımı bu aşamada belirlenmeyecek (kapsam dışı — bkz. Kapsam Tanımı).

## Modül Haritası — Dokümanda Açıkça Tanımlananlar

Doküman madde 17.2 "Yönetim Paneli Modülleri" tablosu tek tek yeniden sayıldı. **Toplam: 15 modül.**

| # | Modül (Dokümandaki Ad) |
|---|---|
| 1 | Dashboard |
| 2 | Sayfa Yönetimi |
| 3 | Ürün Yönetimi |
| 4 | Koleksiyon/Kategori Yönetimi |
| 5 | Blog Yönetimi |
| 6 | Haber Yönetimi |
| 7 | Banner Yönetimi |
| 8 | Referans Proje Yönetimi |
| 9 | Katalog/Doküman Yönetimi |
| 10 | Bayi/Showroom Yönetimi |
| 11 | Form Yönetimi |
| 12 | Dil Yönetimi |
| 13 | SEO Yönetimi |
| 14 | Kullanıcı/Rol Yönetimi |
| 15 | Excel Import |

**Dokümanda tespit edilen iç tutarsızlık (bilgi amaçlı, karar gerektirmiyor):** Madde 17.2 tablosu "Koleksiyon/Kategori Yönetimi"ni tek modül sayıyor; ancak madde 19 (Kategori Yönetimi) ve madde 20 (Koleksiyon Yönetimi) dokümanda ayrı başlıklar ve ayrı veri modelleriyle detaylandırılmış. Modül sayımında (15) doküman madde 17.2'nin kendi tablosu esas alınmıştır.

## Roller — Dokümanda Açıkça Tanımlananlar (Madde 7.2 + Madde 30)

| Rol | Yetki Kapsamı (doküman metni) |
|---|---|
| Admin | Tüm modüller, kullanıcı yönetimi, sistem ayarları, dil yönetimi. |
| İçerik Editörü | Sayfa, blog, haber, banner, referans proje, ürün açıklamaları üzerinde düzenleme yetkisi. |
| SEO Editörü | Meta alanları, URL yönetimi, redirect, schema, sitemap, robots.txt. |
| Ürün Yöneticisi | Ürün ekleme/düzenleme, Excel import, görsel yükleme, doküman ilişkilendirme. |

*(İçerik Editörü satırındaki "onay akışı" notu için bkz. Düzeltme Günlüğü #2 — kaldırıldı, gereksinim olarak işlenmiyor.)*

## Güvenlik Gereksinimleri

**Dokümanda Açıkça Zorunlu Olanlar (Madde 31 + Madde 29.4):** HTTPS, XSS koruması, SQL Injection koruması (parametreli sorgu/ORM), CSRF anti-forgery doğrulama, Authentication (güçlü parola + opsiyonel 2FA), Rate Limiting, File Upload doğrulama (tip/boyut/içerik), KVKK açık rıza, Çerez politikası banner'ı, Loglama, Yedekleme. Form güvenliği (reCAPTCHA v3/honeypot) dokümanda public form gönderimine ait bir gereksinim — Faz 1'de sadece not edilir, uygulaması public site fazındadır.

**Teknik Uygulama Kararı Gerektirenler (henüz seçilmedi, Task 0.2+ konusu):** 2FA yöntemi, rate limiting mekanizması/paketi, loglama altyapısı, CSRF token uygulama şekli, dosya doğrulama kütüphanesi/yöntemi, yedekleme stratejisi/sıklığı.

## Yapılan Mimari Kararlar

Bkz. ARCHITECTURE_DECISIONS.md:
- ADR-001 (Proje kapsamı: Panel + API, public site hariç).
- ADR-002 (Backend API'nin kesin teknik sınırı: sadece panel CRUD/auth/operasyonel ihtiyaçları; public/SAP/CRM endpoint'leri hariç; katmanlar genişletilebilir tasarlanacak — Onaylandı 17.07.2026).
- ADR-003 (Uygulama tipi: server-rendered ASP.NET Core MVC — Views/Controllers, SPA/Razor Pages yok, katmanlı mimari korunacak, authentication yöntemi bu ADR'de KESİNLEŞMEDİ — Onaylandı 17.07.2026).
- ADR-004 (EF Core yaklaşımı: Code First — mevcut/hazır veritabanı yok, migration'lar git ile takip edilecek, çoklu dil/SEO/Showroom kesinleşmeden ilgili entity/migration oluşturulmayacak, BaseEntity/soft-delete/audit/generic repository gibi dokümanda olmayan yapılar otomatik eklenmeyecek — Onaylandı 17.07.2026).
- ADR-005 (Authentication/Authorization: ASP.NET Core Identity + Cookie Authentication — JWT/hibrit yok, Controller/action seviyesinde server-side authorization, 4 rol Role/Claim/Policy ile; 2FA detayları ve şifre sıfırlama akışı bilinçli olarak açık bırakıldı — Onaylandı 17.07.2026).
- ADR-006 (Dosya/görsel/doküman saklama: Yerel Dosya Sistemi + storage abstraction — metadata DB'de, dosya yolu configuration'da, ileride Blob/S3/MinIO'ya geçiş mümkün; çoklu instance riski ADR'de kayıtlı; isimlendirme standardı ve WebP/AVIF/CDN açık bırakıldı — Onaylandı 17.07.2026).
- ADR-007 (Çoklu dil veri modeli: merkezi Translations yaklaşımı/A1, Madde 36.1 esas alındı — EntityType/EntityId polimorfik ilişki, entity-bazlı ayrı tablolar reddedildi; nihai şema detayları ve yetim kayıt temizleme stratejisi açık bırakıldı; fallback davranışı "Gelecek Faz/Karar Bekleniyor" — Onaylandı 17.07.2026).
- ADR-008 (Bayi/Showroom veri modeli: tek Dealer entity + Category ayrımı, ayrı Showroom entity/modül yok; ancak "her şey nullable sütun" yaklaşımı reddedildi — galeri gibi çoklu veriler ilişkili yapıda, gerçek randevu talepleri ayrı operasyonel kayıt olarak modellenecek; kategorisiz kayıtlar ve showroom-özel alan kapsamı açık — Onaylandı 17.07.2026).
- ADR-009 (Public site entegrasyon sınırı: ek iskelet/proje/DTO/contract/senkronizasyon oluşturulmayacak; katman ayrımı (Domain/Application/Infrastructure bağımsızlığı, Controller'lar sadece Application'ı çağırır) yeterli kabul edildi; gelecek fazın kendi gereksinim analizine bırakıldı — Onaylandı 17.07.2026).
- ADR-010 (SAP entegrasyonu: tamamen Faz 2 — SAP Controller/DTO/servis/Client, SyncStatus/SapId gibi alanlar, event/queue/scheduler hiçbiri oluşturulmayacak; ProductCode'un doğal anahtar olması SAP hazırlığı sayılmıyor; ICD kesinleşmeden entegrasyon tahmini yapılmayacak — Onaylandı 17.07.2026).
- ADR-011 (Güvenlik teknik karşılıkları: Rate Limiting = yerleşik middleware (3rd party yok); Loglama = ILogger soyutlaması + Serilog provider, structured logging, hassas bilgi loglanmaz; Dosya Doğrulama = uzantı+MIME+magic-byte+max boyut+whitelist, antivirus yok — Onaylandı 17.07.2026).

## Karar Bekleyen Konular (Açık Kararlar — kesin karar olarak KAYDEDİLMEMİŞTİR)

1. ~~EF Core Code First mi Database First mi~~ — **Çözüldü, bkz. ADR-004 (Code First, Onaylandı 17.07.2026).**
2. ~~Panel authentication yöntemi: Cookie/ASP.NET Identity vs JWT~~ — **Çözüldü, bkz. ADR-005 (Identity + Cookie, Onaylandı 17.07.2026).**
3. ~~Dosya saklama yöntemi: dosya sistemi vs blob storage~~ — **Çözüldü, bkz. ADR-006 (Yerel Dosya Sistemi + storage abstraction, Onaylandı 17.07.2026).**
4. Çoklu dil fallback davranışı (doküman madde 28.3: "Karar Bekleniyor"). **Durum: Gelecek Faz / Karar Bekleniyor (ADR-007 ile teyit edildi — public site kapsamına ait, bu fazın dışında).**
5. ~~Showroom ayrı modül mü, Bayi kategorisi mi~~ — **Çözüldü, bkz. ADR-008 (Tek Dealer entity + Category ayrımı, Onaylandı 17.07.2026).**
6. ~~Güvenlik teknik uygulama kararları~~ — **Çözüldü, bkz. ADR-005 (auth/CSRF) ve ADR-011 (rate limiting/loglama/dosya doğrulama, Onaylandı 17.07.2026).** Yedekleme stratejisi ayrık: bir hosting/operasyon kararı, deployment fazına bırakıldı.
7. ~~SAP API (madde 39) kapsamının Faz 1/Faz 2 netliği~~ — **Çözüldü, bkz. ADR-010 (Tamamen Faz 2, Onaylandı 17.07.2026).**
8. Görsel/dosya isimlendirme standardı (doküman madde 37.4: "Karar Bekleniyor" — ADR-006'da da tekrar teyit edilmiştir).
9. Dokümanın kendi Madde 42 "Açık Konular" tablosundaki diğer 13 madde (sayfalama/infinite scroll, benzer ürün mantığı, RTL desteği kapsamı vb.) — henüz değerlendirilmedi, Task 0.2+'da ele alınacak.
10. **(ADR-005'ten)** 2FA ekranları, aktivasyon süreci, recovery code yönetimi ve zorunluluk politikası — gelecekte ayrı bir task.
11. **(ADR-005'ten)** Şifre sıfırlama akışının bu fazda uygulanıp uygulanmayacağı + e-posta sağlayıcısı/mail gönderim altyapısı seçimi.
12. **(ADR-006'dan)** WebP/AVIF dönüşümü, responsive image üretimi ve CDN kullanımı — ayrı alt karar/task.
13. **(ADR-006'dan, risk)** Çoklu instance deployment senaryosunda yerel dosya sisteminin yetersiz kalma riski — deployment kararına bağlı.
14. ~~Translations tablosunun nihai şeması: FieldName yapısı, Value tipi/uzunluğu, alan-bazlı/tek-kayıt modeli, EntityType temsili, unique index/constraint detayları~~ — **Çözüldü, bkz. ARCHITECTURE_DECISIONS.md ADR-012 (Onaylandı 19.07.2026, Task 3.1A).**
15. ~~Polimorfik EntityId ilişkisi nedeniyle FK kısıtı sınırlaması ve yetim çeviri kaydı temizleme stratejisi~~ — **Çözüldü, bkz. ADR-012 (Application katmanı, aynı transaction, trigger/job yok).** Polimorfik yapının kendisinden kaynaklanan DB-seviyesi FK eksikliği kalıcı bir yapısal risk olarak ADR-012'de kayıtlı kalmaya devam ediyor — bu bir "açık karar" değil, bilinçli olarak kabul edilmiş bir mimari sınırlamadır.
16. **(Yeni, ADR-012'den)** `Language` entity'sinin ayrıntılı alan listesi ve implementasyonu (entity/DbContext/configuration/migration) — henüz yapılmadı, Task 3.1B'nin konusu.
16. **(ADR-008'den)** Kategorisiz (17) Bayi/Showroom kaydının nasıl ele alınacağı — veri aktarımı task'ında.
17. **(ADR-008'den)** Showroom-özel alanların (galeri görselleri, çalışma saatleri, randevu talep formu) bu fazda uygulanıp uygulanmayacağı — Bayi/Showroom modülü task'ında kesinleşecek.
18. **(ADR-011'den)** Rate limiting'in gerçek limit değerleri — ilgili güvenlik task'ında.
19. **(ADR-011'den)** Log hedefleri (dosya/SQL/Seq vb.) — deployment kararında.
20. Yedekleme stratejisi/sıklığı — hosting/operasyon kararı, deployment fazına bırakıldı, mimari karar değil.
21. Üretim veritabanına migration uygulama yöntemi (ADR-004'te not edilmişti) — sonraki bir deployment kararında.

## Sonraki Task

Henüz adlandırılmadı/onaylanmadı. Kategori (4.1), Koleksiyon (4.2), Ürün çekirdek CRUD (5), Ürün Görselleri (5.1) ve Katalog/Doküman Yönetimi (Task 6) tamamlandı — Faz 1 backlog'unda sırada Referans Proje Yönetimi (#10) veya SEO veri sözleşmesi (#4) var (bkz. TASKS.md). Kurulan desenler (Application katmanı feature-folder yapısı, repository/service/unit-of-work mimarisi, `[Authorize(Roles=...)]` RBAC deseni, `IFileStorageService`/ADR-013 dosya depolama deseni, **artık M2M junction-tablo deseni de — ADR-014**) sonraki içerik modüllerinde doğrudan tekrar kullanılabilir/örnek alınabilir.

**Task 5/5.1'den kalan bilinçli açık konu (Task 6'da değişmedi):** Madde 30'un Ürün Yönetimi için istediği alan-seviyeli kısmi RBAC (İçerik Editörü: yalnızca açıklama/görsel; SEO Editörü: yalnızca meta alanları) uygulanmadı. **Not:** Katalog/Doküman modülünde (Task 6) ise Madde 30'un kendi açık satırı olduğu için action-seviyeli bir ayrım (İçerik Editörü: yalnızca Yükleme) uygulandı — bu iki modül arasındaki RBAC farkı bilinçli, dokümanın kendisinden kaynaklanıyor.

**Task 10 (Referans Proje Yönetimi) için not:** Madde 23.1'in `RelatedProducts relation(many-to-many)` alanı, Task 6'da kurulan `ProductDocument`/`CollectionDocument` junction-tablo desenine birebir benzer bir `ProductProject` junction'ı gerektirecek. Referans Proje'nin kendi görselleri de olacağı için (Madde 23.1 `Images gallery`) Task 5.1'in `ProductImage`/`IFileStorageService` deseni de doğrudan örnek alınabilir. `Document` gibi mi (ilişkisi opsiyonel/M2M) yoksa `ProductImage` gibi mi (tek sahiplik) modelleneceği doküman analiziyle netleştirilmeli — henüz yapılmadı.

*(Önceki not korunuyor: Task 2.2 ile authorization altyapısı — rol-bazlı `[Authorize]`, organik AccessDenied, test kullanıcısı — hazır ve doğrulanmış durumda.)*
